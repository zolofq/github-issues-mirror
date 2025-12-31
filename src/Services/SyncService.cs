namespace github_issues_mirror.Services;

using Microsoft.EntityFrameworkCore;

public class SyncService(GitHubIssueService github, IssuesContext db)
{
    public async Task MirrorGithubToLocalAsync(string owner, string repo)
    {
        var issuesJson = await github.GetIssuesAsync(owner, repo);

        foreach (var token in issuesJson)
        {
            var issueId = (long)token["id"]!;
            var existing = await db.Issues.Include(i => i.Comments)
                .FirstOrDefaultAsync(i => i.id == issueId);

            if (existing == null)
            {
                existing = new Issues
                {
                    id = issueId,
                    number = (int)token["number"]!,
                    title = token["title"]!.ToString(),
                    state = token["state"]!.ToString(),
                    updated_at = DateTimeOffset.Parse(token["updated_at"]!.ToString()).UtcDateTime,
                    author = token["user"]?["login"]?.ToString(),
                    body = token["body"]?.ToString()
                };
                db.Issues.Add(existing);
            }
            else
            {
                existing.title = token["title"]!.ToString();
                existing.state = token["state"]!.ToString();
                existing.updated_at = DateTimeOffset.Parse(token["updated_at"]!.ToString()).UtcDateTime;
                existing.body = token["body"]?.ToString();
            }
        }
        await db.SaveChangesAsync();
    }
    
    public async Task SyncLocalToGitHubAsync(long id, string owner, string repo)
    {
        var issue = await db.Issues.FindAsync(id);
        if (issue == null) return;

        await github.UpdateIssueAsync(owner, repo, issue.number, new {
            title = issue.title,
            body = issue.body,
            state = issue.state
        });
    }

    public async Task SyncLocalCommentToGitHubAsync(long id, string owner, string repo)
    {
        var comment = await db.Comments.FindAsync(id);
        if (comment == null) return;

        await github.UpdateCommentAsync(owner, repo, comment.id, new
        {
            body = comment.body,
            updated_at = comment.updated_at
        });
    }
}