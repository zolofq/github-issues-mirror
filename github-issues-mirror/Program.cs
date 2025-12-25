using System;
using System.Net.Http;
using github_issues_mirror;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Sprache;


var builder = WebApplication.CreateBuilder(args);

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
