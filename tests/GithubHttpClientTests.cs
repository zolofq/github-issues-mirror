using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Xunit;

namespace github_issues_mirror.Tests;

public class GithubHttpClientTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly GitHubIssueService _service;

    public GithubHttpClientTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_handlerMock.Object);
        _service = new GitHubIssueService(httpClient);
        github_issues_mirror.Config.Token = "test_token_123";
    }

    private void SetupResponse(HttpStatusCode code, string content = "")
    {
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = code,
                Content = new StringContent(content)
            });
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public async Task ServiceMethods_ThrowException_WhenApiFails(HttpStatusCode code)
    {
        SetupResponse(code);

        await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetIssuesAsync("o", "r"));
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetCommentsAsync("o", "r"));
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.CreateIssueAsync("o", "r", new { }));
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.UpdateIssueAsync("o", "r", 1, new { }));
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.UpdateCommentAsync("o", "r", 1, new { }));
    }

    [Fact]
    public async Task GetIssuesAsync_ReturnsCorrectJson_WhenResponseIsSuccessful()
    {
        SetupResponse(HttpStatusCode.OK, "[{'id': 1, 'title': 'Test Issue'}]");

        var result = await _service.GetIssuesAsync("owner", "repo");

        Assert.NotNull(result);
        Assert.Equal("Test Issue", result[0]["title"].ToString());
    }

    [Fact]
    public async Task UpdateIssueAsync_SendsCorrectJsonRequest()
    {
        string capturedJson = null;

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, token) =>
            {
                if (req.Content != null)
                {
                    capturedJson = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            })
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        await _service.UpdateIssueAsync("owner", "repo", 42, new { body = "New Body" });

        Assert.Equal("{\"body\":\"New Body\"}", capturedJson);

        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Patch),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task CreateIssueAsync_SendsCorrectPostRequest()
    {
        string capturedJson = null;
        var issueData = new { title = "Bug report", body = "Something is broken" };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, token) =>
            {
                if (req.Content != null)
                {
                    capturedJson = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            })
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        await _service.CreateIssueAsync("owner", "repo", issueData);

        Assert.Equal("{\"title\":\"Bug report\",\"body\":\"Something is broken\"}", capturedJson);

        _handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [Fact]
    public async Task GetCommentAsync_ReturnsCorrectJson_WhenResponseIsSuccessful()
    {
        SetupResponse(HttpStatusCode.OK, "[{'id': 1, 'body': 'Test Comment'}]");

        var result = await _service.GetCommentsAsync("owner", "repo");

        Assert.NotNull(result);
        Assert.Equal("Test Comment", result[0]["body"].ToString());
    }

    [Fact]
    public async Task UpdateCommentAsync_SendsCorrectJsonRequest()
    {
        string capturedJson = null;

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, token) =>
            {
                if (req.Content != null)
                {
                    capturedJson = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            })
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        await _service.UpdateCommentAsync("owner", "repo", 42, new { body = "New Body" });

        Assert.Equal("{\"body\":\"New Body\"}", capturedJson);

        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Patch),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}