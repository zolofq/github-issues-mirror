namespace github_issues_mirror.Services;

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

public class SyncService(GitHubIssueService github, IssuesContext db)
{
    public async Task MirrorGithubToLocalAsync(string owner, string repo)
    {
        var issuesJson = await github.GetIssuesAsync(owner, repo);
        var commentsJson = await github.GetCommentsAsync(owner, repo);

        var remoteIssueIds = issuesJson.Select(t => (long)t["id"]);

        // Delete issues which don't exist
        var issuesToDelete = db.Issues.Where(i => !remoteIssueIds.Contains(i.id));
        db.Issues.RemoveRange(issuesToDelete);

        foreach (var token in issuesJson)
        {
            await UpsertIssueFromTokenAsync(token);
        }
        
        await db.SaveChangesAsync();
        
        var remoteCommentIds = commentsJson.Select(c => (long)c["id"]!).ToList();

        // Delete comments which don't exist
        var commentsToDelete = db.Comments.Where(c => !remoteCommentIds.Contains(c.id));
        db.Comments.RemoveRange(commentsToDelete);

        foreach (var cToken in commentsJson)
        {
            var issueUrl = cToken["issue_url"]?.ToString();
            if (string.IsNullOrEmpty(issueUrl)) continue;

            if (int.TryParse(issueUrl.Split('/').Last(), out int issueNumber))
            {
                var issue = await db.Issues.FirstOrDefaultAsync(i => i.number == issueNumber);
            
                if (issue != null)
                {
                    await UpsertCommentFromTokenAsync(cToken, issue.id);
                }
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task UpsertIssueFromTokenAsync(JToken token)
    {
        var issueId = (long)token["id"];
        var existing = await db.Issues.FirstOrDefaultAsync(i => i.id == issueId);

        if (existing == null)
        {
            existing = new Issues { id = issueId };
            db.Issues.Add(existing);
        }

        existing.number = (int)token["number"];
        existing.title = token["title"]?.ToString() ?? "";
        existing.state = token["state"]?.ToString() ?? "open";
        existing.body = token["body"]?.ToString();
        existing.author = token["user"]?["login"]?.ToString();
        existing.updated_at = DateTimeOffset.Parse(token["updated_at"]!.ToString()).UtcDateTime;
    }

    public async Task UpsertCommentFromTokenAsync(JToken cToken, long issueId)
    {
        var commentId = (long)cToken["id"];
        var existingComment = await db.Comments.FirstOrDefaultAsync(c => c.id == commentId);

        if (existingComment == null)
        {
            existingComment = new Comments { id = commentId };
            db.Comments.Add(existingComment);
        }

        existingComment.body = cToken["body"]?.ToString();
        existingComment.author = cToken["user"]?["login"]?.ToString();
        existingComment.updated_at = DateTimeOffset.Parse(cToken["updated_at"]!.ToString()).UtcDateTime;


        existingComment.Issueid = issueId;
    }

    public async Task SyncLocalToGitHubAsync(long id, string owner, string repo)
    {
        var issue = await db.Issues.FindAsync(id);
        if (issue == null) return;

        await github.UpdateIssueAsync(owner, repo, issue.number, new
        {
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

    public async Task CreateIssueFromLocalAsync(long number, string owner, string repo)
    {
        var issue = await db.Issues.FirstOrDefaultAsync(i => i.number == number);

        if (issue == null)
        {
            throw new KeyNotFoundException($"Issue with ID {number} not found in local database.");
        }

        await github.CreateIssueAsync(owner, repo, new
        {
            title = issue.title,
            body = issue.body
        });
    }

    public async Task CreateCommentFromLocalAsync(long commentId, long id, string owner, string repo)
    {
        var comment = await db.Comments.FirstOrDefaultAsync(c => c.id == commentId);
        if (comment == null) return;

        await github.CreateCommentAsync(owner, repo, id, new
        {
            body = comment.body,
            updated_at = comment.updated_at
        });
    }

    public async Task DeleteCommentFromLocalAsync(long id, string owner, string repo)
    {
        var comment = await db.Comments.FindAsync(id);
        if (comment == null) return;

        await github.DeleteCommentAsync(owner, repo, id);
        db.Comments.Remove(comment);
        await db.SaveChangesAsync();
    }
}