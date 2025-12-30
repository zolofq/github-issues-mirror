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

        public async Task<JToken> GetCommentsAsync(string owner, string repo, int issueNumber) =>
            await SendRequestAsync(HttpMethod.Get, $"repos/{owner}/{repo}/issues/{issueNumber}/comments")
            ?? throw new Exception("Empty response");

        public async Task UpdateCommentAsync(string owner, string repo, int issueId, int commentId, object data) =>
            await SendRequestAsync(HttpMethod.Patch, $"repos/{owner}/{repo}/issues/{issueId}/comments/{commentId}",
                data);
    }
}