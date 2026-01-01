namespace github_issues_mirror.Endpoints;

using github_issues_mirror.Services;

public static class GitHubEndpoints
{
    public static void MapGitHubEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/github");

        group.MapGet("/mirror", async (SyncService syncService) =>
        {
            await syncService.MirrorGithubToLocalAsync(Config.GH_Username, Config.GH_Repository);
            return Results.Ok();
        });
        
        group.MapPost("/sync/{id}", async (long id, SyncService syncService) =>
        {
            await syncService.SyncLocalToGitHubAsync(id, Config.GH_Username, Config.GH_Repository);
            return Results.Ok();
        });

        group.MapPost("/sync/comment/{id}", async (long id, SyncService syncService) =>
        {
            await syncService.SyncLocalCommentToGitHubAsync(id, Config.GH_Username, Config.GH_Repository);
            return Results.Ok();
        });

        group.MapPost("/create/{number}", async (long number, SyncService syncService) =>
        {
            await syncService.CreateIssueFromLocalAsync(number, Config.GH_Username, Config.GH_Repository);
            return Results.Ok();
        });

        group.MapPost("/create/{issueNumber}/comments/{commentId}", async (long issueNumber, long commentId, SyncService syncService) =>
        {
            await syncService.CreateCommentFromLocalAsync(issueNumber, commentId, Config.GH_Username, Config.GH_Repository);
            return Results.Ok(new { status = "Success", issueId = issueNumber, localCommentId = commentId });
        });

        group.MapDelete("/delete/comments/{id}", async (long id, SyncService SyncService) =>
        {
            await SyncService.DeleteCommentFromLocalAsync(id, Config.GH_Username, Config.GH_Repository);
            return Results.Ok();
        });
    }
}