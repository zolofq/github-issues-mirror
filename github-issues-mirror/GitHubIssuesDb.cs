using Microsoft.EntityFrameworkCore;
using github_issues_mirror;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace github_issues_mirror
{
    public class GithubIssuesDb : DbContext
    {
        public DbSet<Issues> Issues { get; set; }
        public GithubIssuesDb()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql($"Host=localhost;Port=5433;Database=githubIssues;Username={DbConfig.Username};Password={DbConfig.Password}");
        }
    }
}
