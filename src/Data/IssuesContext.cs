using Microsoft.EntityFrameworkCore;
using github_issues_mirror;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace github_issues_mirror
{
    public class IssuesContext : DbContext
    {
        public DbSet<Issues> Issues { get; set; }
        public DbSet<Comments> Comments { get; set; }
        public IssuesContext()
        {
            Database.EnsureCreated(); 
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dockerConnString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            if (!string.IsNullOrEmpty(dockerConnString))
            {
                optionsBuilder.UseNpgsql(dockerConnString);
            }
            else
            {
                optionsBuilder.UseNpgsql($"Host=localhost;Port=5432;Database=githubIssues;Username={Config.Username};Password={Config.Password}");
            }
        }
    }
}
