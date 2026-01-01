namespace github_issues_mirror;

using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public abstract class GitHubClientBase
{
    protected readonly HttpClient _httpClient;

    private readonly string _token;

    protected GitHubClientBase(HttpClient httpClient, string token)
    {
        _httpClient = httpClient;
        _token = token;
    }

    protected async Task<JToken?> SendRequestAsync(HttpMethod method, string endpoint, object? data = null)
    {
        var url = $"https://api.github.com/{endpoint.TrimStart('/')}";
        using var request = new HttpRequestMessage(method, url);
        
        if (!string.IsNullOrEmpty(Config.Token))
        {
            request.Headers.Add("Authorization", $"Bearer {Config.Token}");
        }

        if (!string.IsNullOrEmpty(Config.GH_Repository))
        {
            request.Headers.Add("User-Agent", Config.GH_Repository);
        }

        if (data != null)
        {
            var json = JsonConvert.SerializeObject(data);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        using var response = await _httpClient.SendAsync(request);
        
        response.EnsureSuccessStatusCode();

        if (method == HttpMethod.Get)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            return JToken.Parse(jsonData);
        }
        
        return null;
    }
}