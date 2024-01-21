namespace InterviewPrepApi.Models
{
	public class Question
	{
		public int id { get; set; }
		public string title { get; set; }
		public string description { get; set; }
		public string? pythonDefault { get; set; }
		public string? pythonTests { get; set; }
	}
}
