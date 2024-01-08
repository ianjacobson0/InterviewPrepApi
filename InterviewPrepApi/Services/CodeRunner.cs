using Docker.DotNet;
using Docker.DotNet.BasicAuth;
using Docker.DotNet.Models;

using InterviewPrepApi.DTO;

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

			// create container
			var startParams = new ContainerStartParameters();
			var createResponse = await client.Containers.CreateContainerAsync(new CreateContainerParameters()
			{
				Image = "alpine",
				AttachStdin = true,
				AttachStderr = true,
				NetworkDisabled = true,
				Tty = true
			});

			var startResponse = await client.Containers.StartContainerAsync(createResponse.ID, startParams);

			// exec container
			var createExecParameters = new ContainerExecCreateParameters()
			{
				AttachStdout = true,
				AttachStdin = true,
				Cmd = new List<string> { "echo", "Hello" },
				Privileged = false
			};

			var startExecParameters = new ContainerExecStartParameters
			{
				Detach = false,
				Tty = false,
				Privileged = false
			};

			var createExecResponse = await client.Exec.ExecCreateContainerAsync(createResponse.ID, createExecParameters);

			using var startStream = await client.Exec.StartWithConfigContainerExecAsync(createExecResponse.ID, startExecParameters);
				
			var res = await startStream.ReadOutputToEndAsync(CancellationToken.None);

			await client.Containers.KillContainerAsync(createResponse.ID, new ContainerKillParameters());
			return new CodeResponseDTO { Success = true, StdOut = res.stdout, StdErr = res.stderr };
		}
	}
}
