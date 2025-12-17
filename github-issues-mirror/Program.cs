using github_issues_mirror;

var builder = WebApplication.CreateBuilder(args);

DbConfig.Username = builder.Configuration["username"] ?? throw new Exception("Username is required");
DbConfig.Password = builder.Configuration["password"] ?? throw new Exception("Passowrd is required");

var app = builder.Build();

app.MapGet("/github-issues/dotnet/aspnetcore", async () =>
{
    try
    {
        string issues = await GithubHttpClient.GetIssuesAsync("dotnet", "aspnetcore");
        return Results.Text(issues, "application/json");
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem($"Error while request to GitHubApi:{ex.Message}");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}");
    }
});

app.Run();
