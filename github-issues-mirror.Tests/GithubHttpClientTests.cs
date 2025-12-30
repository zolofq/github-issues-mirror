using Moq;
using Xunit;

namespace github_issues_mirror.Tests;

using System.Net;
using Moq.Protected;
using Newtonsoft.Json.Linq;

public class GithubHttpClientTests
{
    [Fact]
    public async Task GetIssuesAsync_ReturnsCorrectJson_WhenResponseIsSuccessful()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var expectedJson = "[{'id': 1, 'title': 'Test Issue'}]";

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedJson)
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new GitHubIssueService(httpClient);
        var result = await service.GetIssuesAsync("owner", "repo");

        Assert.NotNull(result);
        // Assert that the first issue has the correct title
        Assert.Equal("Test Issue", result[0]["title"].ToString());

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1), // Verify that SendAsync was called exactly once
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri.ToString().Contains("github.com")), // Verify that the request was a GET request
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetIssuesAssync_ThrowsException_WhenResponseIsNotSuccessful()
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new GitHubIssueService(httpClient);

        // Assert that an exception is thrown when the response is not successful
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            service.GetIssuesAsync("owner", "repo"));
    }

    [Fact]
    public async Task UpdateIssuesAsync_SendsCorrectJsonRequest()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        
        HttpRequestMessage capturedRequest = null;
        string capturedJson = null;
        
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, token) =>
                {
                    capturedRequest = req;
                    if (req.Content != null)
                    {
                        capturedJson = req.Content.ReadAsStringAsync().Result;
                    }
                }
                )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
            });
        
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new GitHubIssueService(httpClient);

        var updateData = new { title = "New Title", state = "closed" };
        
        await service.UpdateIssueAsync("owner", "repo", 42, updateData);
        
        Assert.NotNull(capturedJson);
        Assert.Equal(HttpMethod.Patch, capturedRequest.Method);
        Assert.EndsWith("/issues/42", capturedRequest.RequestUri.ToString());
        
        Assert.Equal("{\"title\":\"New Title\",\"state\":\"closed\"}", capturedJson);
    }

    [Fact]
    public async Task UpdateIssuesAsync_ThrowException_WhenResponseIsNotSuccessful()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new GitHubIssueService(httpClient);
        
        var updateData = new { title = "New Title", state = "closed" };

        // Assert that an exception is thrown when the response is not successful
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            service.UpdateIssueAsync("owner", "repo", 42, updateData));
    }
    
    [Fact]
    public async Task GetCommentsAsync_ReturnsJson_And_SendsCorrectHeaders()
    {
        // --- Arrange ---
        var handlerMock = new Mock<HttpMessageHandler>();
        var expectedJson = "[{'id': 101, 'body': 'First comment'}, {'id': 102, 'body': 'Second comment'}]";
        
        // Устанавливаем тестовый токен, чтобы код не упал при чтении Config.Token
        github_issues_mirror.Config.Token = "test_token_123";

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedJson),
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new github_issues_mirror.GitHubIssueService(httpClient);

        var result = await service.GetCommentsAsync("zolofq", "my-repo", 42);

        Assert.NotNull(result);
        Assert.Equal(2, ((JArray)result).Count);
        Assert.Equal("First comment", result[0]["body"].ToString());

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Get && 
                req.RequestUri.ToString().Contains("/issues/42/comments") &&
                req.Headers.Authorization.Parameter == "test_token_123" &&
                req.Headers.UserAgent.ToString() == "github-issues-mirror"
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetCommentsAsync_ThrowsException_OnServerError()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new github_issues_mirror.GitHubIssueService(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(() => 
            service.GetCommentsAsync("owner", "repo", 1));
    }
}