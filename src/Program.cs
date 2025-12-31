namespace github_issues_mirror;

using github_issues_mirror.Endpoints;
using github_issues_mirror.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        ConfigureServices(builder.Services);

        var app = builder.Build();

        ConfigureEndpoints(app);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddDbContext<IssuesContext>();
        services.AddScoped<GitHubIssueService>();
        services.AddScoped<SyncService>();
    }

    private static void ConfigureEndpoints(WebApplication app)
    {
        app.MapGitHubEndpoints();
    }
}