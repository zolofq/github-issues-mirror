using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace github_issues_mirror
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class GithubHttpClient
    {
        static readonly HttpClient httpClient = new HttpClient();
        
        /// <summary>
        /// Sends an HTTP GET request to the GitHub REST API and retrieves
        /// a list of issues for the specified repository.
        /// </summary>
        /// <param name="owner">
        /// Repository owner (GitHub username or organization name)
        /// </param>
        /// <param name="repo">
        /// Repository name
        /// </param>
        /// <returns></returns>
        public static async Task<JToken> GetIssuesAsync(string owner, string repo)
        {
            string url = $"https://api.github.com/repos/{owner}/{repo}/issues";

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("Authorization", $"Bearer {Config.Token}");
            request.Headers.Add("User-Agent", "github-issues-mirror");

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            string jsonData = await response.Content.ReadAsStringAsync();
            return JToken.Parse(jsonData) ?? throw new Exception("Null reference return in jsonData");
        }

        public static async Task UpdateIssueAsync(string owner, string repo, int issueNumber, object data)
        {
            string url = $"https://api.github.com/repos/{owner}/{repo}/issues/{issueNumber}";

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, url);
            request.Content = content;
            
            request.Headers.Add("Authorization", $"Bearer {Config.Token}");
            request.Headers.Add("User-Agent", "github-issues-mirror");

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }
    }
}
