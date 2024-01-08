using InterviewPrepApi.Auth;
using InterviewPrepApi.Data;
using InterviewPrepApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InterviewPrepApi.DTO;

namespace InterviewPrepApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AuthController : ControllerBase
	{

		private readonly JwtAuthenticationManager authManager;

		public AuthController(JwtAuthenticationManager authManager)
		{
			this.authManager = authManager;
		}

		[AllowAnonymous]
		[HttpPost("Authorize")]
		public IActionResult AuthUser([FromBody] LoginDTO loginDTO)
		{
			string token = authManager.Authenticate(loginDTO.Username, loginDTO.Password);
			if (token == null)
				return Unauthorized();
			return Ok(token);
		}
	}
}