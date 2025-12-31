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
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetCommentsAsync("o", "r", 1));
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
    {
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            {


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
    public async Task GetCommentsAsync_ReturnsJson_And_SendsCorrectHeaders()
    {
        SetupResponse(HttpStatusCode.OK, "[{'id': 101, 'body': 'First comment'}]");

        var result = await _service.GetCommentsAsync("zolofq", "my-repo", 42);

        Assert.NotNull(result);
        Assert.Equal("First comment", result[0]["body"].ToString());

        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri.ToString().Contains("/issues/42/comments") &&
                req.Headers.Authorization.Parameter == "test_token_123"
            ),
            ItExpr.IsAny<CancellationToken>()
        );
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