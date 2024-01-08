namespace InterviewPrepApi.Models
{
	public class User
	{
		public int id { get; set; }
		public string? name { get; set; }
		public string email { get; set; }
		public string hashedPassword { get; set; }
		public string salt { get; set; }
		public string? resetToken { get; set; }
		public DateTime? resetTokenExpiresAt { get; set; }
		public string roles { get; set; }
	}
}
