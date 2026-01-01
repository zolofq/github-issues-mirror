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
    }
}