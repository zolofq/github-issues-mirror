namespace github_issues_mirror
{
    using Newtonsoft.Json.Linq;

    public class GitHubIssueService : GitHubClientBase
    {
        public GitHubIssueService(HttpClient httpClient) : base(httpClient, Config.Token) {}

        public async Task<JToken> GetIssuesAsync(string owner, string repo) =>
            await SendRequestAsync(HttpMethod.Get, $"repos/{owner}/{repo}/issues")
            ?? throw new Exception("Empty response");

        public async Task UpdateIssueAsync(string owner, string repo, int issueNumber, object data) =>
            await SendRequestAsync(HttpMethod.Patch, $"repos/{owner}/{repo}/issues/{issueNumber}", data);

        public async Task CreateIssueAsync(string owner, string repo, object data) =>
            await SendRequestAsync(HttpMethod.Post, $"repos/{owner}/{repo}/issues", data);
        
        public async Task<JToken> GetCommentsAsync(string owner, string repo) =>
            await SendRequestAsync(HttpMethod.Get, $"repos/{owner}/{repo}/issues/comments")
            ?? throw new Exception("Empty response");

        public async Task UpdateCommentAsync(string owner, string repo, long id, object data) =>
            await SendRequestAsync(HttpMethod.Patch, $"repos/{owner}/{repo}/issues/comments/{id}",
                data);
        
        public async Task CreateCommentAsync(string owner, string repo, long id, object data) =>
            await SendRequestAsync(HttpMethod.Post, $"repos/{owner}/{repo}/issues/{id}/comments",
                data);
        
        public async Task DeleteCommentAsync(string owner, string repo, long id) =>
            await SendRequestAsync(HttpMethod.Delete, $"repos/{owner}/{repo}/issues/comments/{id}");
    }
}