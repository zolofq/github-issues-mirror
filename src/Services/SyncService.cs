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
            var existing = await db.Issues
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

            var commentsJson = await github.GetCommentsAsync(owner, repo);

            foreach (var cToken in commentsJson)
            {
                var commentId = (long)cToken["id"]!;
                var existingComment = await db.Comments
                    .FirstOrDefaultAsync(c => c.id == commentId);

                if (existingComment == null)
                {
                    existingComment = new Comments
                    {
                        id = commentId,
                        body = cToken["body"]?.ToString(),
                        author = cToken["user"]?["login"]?.ToString(),
                        updated_at = DateTimeOffset.Parse(cToken["updated_at"]!.ToString()).UtcDateTime
                    };
                    db.Comments.Add(existingComment);
                }
                else
                {
                    existingComment.body = cToken["body"]?.ToString();
                    existingComment.updated_at = DateTimeOffset.Parse(cToken["updated_at"]!.ToString()).UtcDateTime;
                }
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
}