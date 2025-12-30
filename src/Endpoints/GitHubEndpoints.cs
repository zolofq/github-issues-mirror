namespace github_issues_mirror.Endpoints;

using github_issues_mirror.Services;

public static class GitHubEndpoints
{
    public static void MapGitHubEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/github");

        group.MapGet("/mirror", async (SyncService syncService) =>
        {
            await syncService.MirrorGithubToLocalAsync("zolofq", "src");
            return Results.Ok();
        });
        
        group.MapPost("/sync/{id}", async (long id, SyncService syncService) =>
        {
            await syncService.SyncLocalToGitHubAsync(id, "zolofq", "src");
            return Results.Ok();
        });
    }
}