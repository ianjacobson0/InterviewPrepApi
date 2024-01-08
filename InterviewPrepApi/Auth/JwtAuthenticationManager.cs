using InterviewPrepApi.Data;
using InterviewPrepApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InterviewPrepApi.Auth
{
	public class JwtAuthenticationManager
	{
		private readonly string key;
		private readonly IServiceScopeFactory scopeFactory;
		public JwtAuthenticationManager(string key, IServiceScopeFactory scopeFactory)
		{
			this.key = key;
			this.scopeFactory = scopeFactory;
		}

		public string? Authenticate(string username, string password)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
				User? user = context.User.FirstOrDefault(u => u.email == username);
				if (user == null)
					return null;
				string salt = user.salt;
				string hashedPassword = AuthFunctions.EncryptPassword(password, salt);
				if (!context.User.Any(u => u.email == username && u.hashedPassword == hashedPassword))
					return null;
			}

			JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
			byte[] tokenKey = Encoding.ASCII.GetBytes(key);
			SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(ClaimTypes.Name, username)
				}),
				Expires = DateTime.UtcNow.AddHours(1),
				SigningCredentials = new SigningCredentials(
					new SymmetricSecurityKey(tokenKey),
					SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
	}
}
