using System;
using System.Net.Http;
using github_issues_mirror;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Sprache;


var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/github-issues/zolofq/github-issues-mirror", async () =>
{
    try
    {
        var jArray = await GithubHttpClient.GetIssuesAsync("zolofq", "github-issues-mirror");

        if (jArray == null)
        {
            return Results.Problem("No data received from GitHub API");
        }
        
        await using var db = new IssuesContext();
        
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        
        foreach (var issue in jArray)
        {
            if (issue == null) throw new Exception("issue null reference");

            var issueId = (long)issue["id"]!;

            var dbIssue = db.Issues.FirstOrDefault(x => x.id == issueId);

            if (dbIssue == null)
            {
                dbIssue = new Issues
                {
                    id = issueId,
                    number = (int)issue["number"]!,
                    title = issue["title"]!.ToString(),
                    state = issue["state"]!.ToString(),
                    updated_at = DateTimeOffset.Parse(issue["updated_at"]!.ToString()).UtcDateTime,
                    author = issue["user"]?["login"]?.ToString(),
                    body = issue["body"]?.ToString()
                };
                db.Issues.Add(dbIssue);
            }
            else
            {
                dbIssue.title = issue["title"]!.ToString();
                dbIssue.state = issue["state"]!.ToString();
                dbIssue.updated_at = DateTimeOffset.Parse(issue["updated_at"]!.ToString()).UtcDateTime;
                dbIssue.body = issue["body"]?.ToString();
            }
        }
        
        await db.SaveChangesAsync();
        
        return Results.Ok();
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem($"Error while request to GitHubApi:{ex.ToString()}");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error:{ex.ToString()}");
    }
});

app.MapPost("/sync-to-github/{id}", async (long id) =>
{
    await using var db = new IssuesContext();
    var issue = db.Issues.FirstOrDefault(x => x.id == id);
    if (issue == null) return Results.NotFound();

    try
    {
        var updateData = new
        {
            title = issue.title,
            body = issue.body,
            state = issue.state
        };

        await GithubHttpClient.UpdateIssueAsync("zolofq", "github-issues-mirror", issue.number, updateData);

        return Results.Ok("Github Issue updated successfully");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to sync: {ex.Message}");
    }
});

app.Run();
