using Microsoft.AspNetCore.Mvc;

namespace InterviewPrepApi.DTO
{
	public class CodeResponseDTO
	{
		public bool Correct { get; set; }
		public string? ErrorMessage { get; set; }
		public string? StdOut { get; set; }
		public string? StdErr { get; set; }
	}
}
