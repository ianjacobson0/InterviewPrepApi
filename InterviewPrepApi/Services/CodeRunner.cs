using Docker.DotNet;
using Docker.DotNet.BasicAuth;
using Docker.DotNet.Models;
using ICSharpCode.SharpZipLib.Tar;
using InterviewPrepApi.DTO;
using Microsoft.AspNetCore.Components.Server;
using Newtonsoft.Json;
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace InterviewPrepApi.Services
{
	public class CodeRunner
	{
		public async Task<CodeResponseDTO> Run(CodeDTO codeDTO)
		{
			switch (codeDTO.Type)
			{
				case "python":
					return await RunPython(codeDTO.Source);
				default:
					return null;
			}
		}

		private async Task<CodeResponseDTO> RunPython(string code)
		{
			using var client = new DockerClientConfiguration(new Uri("tcp://host.docker.internal:2375"), defaultTimeout: TimeSpan.FromMinutes(5)).CreateClient();
			Debug.WriteLine($"code {code}");

			// create files
			string contextPath = @"./context";
			Directory.CreateDirectory(contextPath);

			string pythonPath = @"./context/code.py";
			var codeFile = File.Create(pythonPath);
			codeFile.Close();
			File.WriteAllText(pythonPath, code);

			string dockerPath = @"./context/Dockerfile";
			var dockerFile = File.Create(dockerPath);
			dockerFile.Close();
			var sb = new StringBuilder();
			sb.AppendLine("FROM python:3");
			sb.AppendLine("COPY ./code.py ./code.py");
			sb.AppendLine("ENTRYPOINT python ./code.py");
			File.WriteAllText(dockerPath, sb.ToString());

			// build image
			string imageName = "python-image";
			var imageParameters = new ImageBuildParameters()
			{
				Tags = new List<string> { imageName },
			};
			var stream = CreateTarballForDockerfileDirectory(contextPath);
			using (var responseStream = await client.Images.BuildImageFromDockerfileAsync(stream, imageParameters))
			{
				using (var reader = new StreamReader(responseStream))
				{
					while (!reader.EndOfStream)
					{
						string line = reader.ReadLine();
						if (line != null)
							Debug.WriteLine(line);
					}
				}
			}

			// delete python file and dockerfile
			File.Delete(pythonPath);
			File.Delete(dockerPath);

			// push image
			var pushParameters = new ImagePushParameters { Tag = imageName };
			Progress<JSONMessage> progress = new Progress<JSONMessage>();
			progress.ProgressChanged += (sender, value) => Debug.WriteLine($"Pushing {imageName}");
			await client.Images.PushImageAsync(imageName, pushParameters, new AuthConfig(), progress);
			Debug.WriteLine($"{imageName} pushed");

			// create and start container
			var createParameters = new CreateContainerParameters()
			{
				Image = imageName,
				Tty = false,
				AttachStdout = true,
				AttachStderr = true,
				NetworkDisabled = true
			};
			var createResponse = await client.Containers.CreateContainerAsync(createParameters);
			string containerId = createResponse.ID;
			var startResponse = await client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
			Debug.WriteLine($"{containerId} started");

			// get container logs
			ContainerInspectResponse inspectResponse = null;
			do
			{
				inspectResponse = await client.Containers.InspectContainerAsync(containerId);
				await Task.Delay(100);

			} while (inspectResponse != null && inspectResponse.State.Running == true);

			var logParameters = new ContainerLogsParameters()
			{
				ShowStderr = true,
				ShowStdout = true,
				Follow = false
			};

			MultiplexedStream logStream = await client.Containers.GetContainerLogsAsync(containerId, false, logParameters);
			var result = await logStream.ReadOutputToEndAsync(CancellationToken.None);
			logStream.Dispose();
			Debug.WriteLine($"{containerId} gathered logs");

			// stop container
			await client.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
			Debug.WriteLine($"{containerId} stopped");

			// delete image
			await client.Images.DeleteImageAsync(imageName, new ImageDeleteParameters { Force = true});
			Debug.WriteLine($"{imageName} deleted");

			return new CodeResponseDTO { Success = true, StdOut = result.stdout, StdErr = result.stderr, ErrorMessage = null };
		}
		private static Stream CreateTarballForDockerfileDirectory(string directory)
		{
			var tarball = new MemoryStream();
			var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);

			using var archive = new TarOutputStream(tarball)
			{
				//Prevent the TarOutputStream from closing the underlying memory stream when done
				IsStreamOwner = false
			};

			foreach (var file in files)
			{
				//Replacing slashes as KyleGobel suggested and removing leading /
				string tarName = file.Substring(directory.Length).Replace('\\', '/').TrimStart('/');

				//Let's create the entry header
				var entry = TarEntry.CreateTarEntry(tarName);
				using var fileStream = File.OpenRead(file);
				entry.Size = fileStream.Length;
				archive.PutNextEntry(entry);

				//Now write the bytes of data
				byte[] localBuffer = new byte[32 * 1024];
				while (true)
				{
					int numRead = fileStream.Read(localBuffer, 0, localBuffer.Length);
					if (numRead <= 0)
						break;

					archive.Write(localBuffer, 0, numRead);
				}

				//Nothing more to do with this entry
				archive.CloseEntry();
			}
			archive.Close();

			//Reset the stream and return it, so it can be used by the caller
			tarball.Position = 0;
			return tarball;
		}
	}
}
