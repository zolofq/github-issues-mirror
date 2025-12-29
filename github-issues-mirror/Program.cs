using System;
using System.Net.Http;
using github_issues_mirror;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Sprache;


var builder = WebApplication.CreateBuilder(args);

// Configure dependency injection for http client and github http client
builder.Services.AddHttpClient(); 
builder.Services.AddScoped<GithubHttpClient>();

var app = builder.Build();

app.MapGet("/github-issues/zolofq/github-issues-mirror", async (GithubHttpClient githubHttpClient) =>
{
    try
    {
        var jArray = await githubHttpClient.GetIssuesAsync("zolofq", "github-issues-mirror");

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

            var commentsData = await githubHttpClient.GetCommentsAssync("zolofq", "github-issues-mirror", dbIssue.number);

            foreach (var commentToken in commentsData)
            {
                var commentId = (long)commentToken["id"];

                var dbComment = db.Comments.FirstOrDefault(c => c.id == commentId);

                if (dbComment == null)
                {
                    dbComment = new Comments
                    {
                        id = commentId,
                        issue_id = issueId,
                        author = commentToken["user"]?["login"]?.ToString() ?? "unknown",
                        body = commentToken["body"]?.ToString() ?? "",
                        updated_at = DateTimeOffset.Parse(commentToken["updated_at"]!.ToString()).UtcDateTime
                    };
                    dbIssue.Comments.Add(dbComment);
                }
                else
                {
                    dbComment.id = commentId;
                    dbComment.issue_id = issueId;
                    dbComment.author = commentToken["user"]?["login"]?.ToString() ?? "unknown";
                    dbComment.body = commentToken["body"]?.ToString() ?? "";
                    dbComment.updated_at = DateTimeOffset.Parse(commentToken["updated_at"]!.ToString()).UtcDateTime;
                }
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

app.MapPost("/sync-to-github/{id}", async (long id, GithubHttpClient githubHttpClient) =>
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

        await githubHttpClient.UpdateIssueAsync("zolofq", "github-issues-mirror", issue.number, updateData);

        return Results.Ok("Github Issue updated successfully");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to sync: {ex.Message}");
    }
});

app.Run();
