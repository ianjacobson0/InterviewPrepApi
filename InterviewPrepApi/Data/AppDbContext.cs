using InterviewPrepApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InterviewPrepApi.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{

		}

		public DbSet<User>? User { get; set; }
		public DbSet<Question>? Question { get; set; }
	}
}
