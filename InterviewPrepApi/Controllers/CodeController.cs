using InterviewPrepApi.DTO;
using InterviewPrepApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InterviewPrepApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class CodeController : Controller
	{
		private readonly CodeRunner codeRunner;
		public CodeController(CodeRunner codeRunner)
		{
			this.codeRunner = codeRunner;
		}

		[AllowAnonymous]
		[HttpPost("run")]
		public async Task<IActionResult> RunCode([FromBody] CodeDTO codeDTO)
		{
			var res = await codeRunner.Run(codeDTO);
			return Ok(res);
		}
	}
}
