using Microsoft.EntityFrameworkCore;
using github_issues_mirror;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace github_issues_mirror
{
    public class IssuesContext : DbContext
    {
        public DbSet<Issues> Issues { get; set; }
        public IssuesContext()
        {
            Database.EnsureCreated(); 
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql($"Host=localhost;Port=5432;Database=githubIssues;Username={Config.Username};Password={Config.Password}");
        }
    }
}
