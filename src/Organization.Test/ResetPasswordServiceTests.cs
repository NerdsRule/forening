using System.Net;
using System.Text;
using System.Text.Json;
using Organization.Infrastructure.Services;
using Organization.Shared.DatabaseObjects;
using Organization.Shared.Interfaces;

namespace Organization.Test;

public class ResetPasswordServiceTests
{
    [Fact]
    public async Task RequestPasswordResetAsync_PostsModelToExpectedEndpoint()
    {
        // Capture the outgoing request so endpoint and payload can be asserted.
        HttpRequestMessage? capturedRequest = null;
        var responseJson = JsonSerializer.Serialize(
            new FormResult { Succeeded = true },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var service = CreateService(request =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };
        });

        var result = await service.RequestPasswordResetAsync(new RequestPasswordResetModel { Email = "test@example.com" });

        result.Succeeded.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.RequestUri!.PathAndQuery.Should().Be("/v1/api/users/password/requestOfResetPassword");

        var payload = await capturedRequest.Content!.ReadFromJsonAsync<RequestPasswordResetModel>(cancellationToken: TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetResetRequestsAsync_WhenApiReturnsOk_ReturnsDataAndSucceededResult()
    {
        // Return serialized list to verify successful data round-trip.
        var expected = new List<TResetPassword>
        {
            new() { Id = 7, AppUserId = "u1", ResetRequestCount = 1 },
            new() { Id = 8, AppUserId = "u2", ResetRequestCount = 2 },
        };
        var responseJson = JsonSerializer.Serialize(expected, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var service = CreateService(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            });

        var (data, result) = await service.GetResetRequestsAsync(42, TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Succeeded.Should().BeTrue();
        data.Should().NotBeNull();
        data!.Select(x => x.Id).Should().Contain([7, 8]);
    }

    [Fact]
    public async Task GetResetRequestsAsync_WhenApiReturnsFailure_ReturnsFailedFormResult()
    {
        // Non-success status should not attempt to deserialize data list.
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));

        var (data, result) = await service.GetResetRequestsAsync(42, TestContext.Current.CancellationToken);

        data.Should().BeNull();
        result.Should().NotBeNull();
        result!.Succeeded.Should().BeFalse();
        result.ErrorList.Should().Contain("Failed to retrieve reset requests.");
    }

    [Fact]
    public async Task DeleteResetRequestAsync_WhenApiReturnsOkAndNoBody_ReturnsSucceededTrue()
    {
        // Service treats empty successful responses as success acknowledgements.
        var service = CreateService(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty),
            });

        var result = await service.DeleteResetRequestAsync(10, 5, TestContext.Current.CancellationToken);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteResetRequestAsync_WhenHandlerThrows_ReturnsFallbackError()
    {
        // Network or serialization exceptions should be converted to user-safe fallback errors.
        var service = CreateService(_ => throw new HttpRequestException("network down"));

        var result = await service.DeleteResetRequestAsync(10, 5, TestContext.Current.CancellationToken);

        result.Succeeded.Should().BeFalse();
        result.ErrorList.Should().Contain("An unknown error prevented the reset request deletion from succeeding.");
    }

    private static IResetPasswordService CreateService(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        var handler = new StubHttpMessageHandler(responseFactory);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:7375"),
        };

        var httpClientFactory = new StubHttpClientFactory(httpClient);
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<ResetPasswordService>();

        return new ResetPasswordService(httpClientFactory, logger);
    }

    private sealed class StubHttpClientFactory(HttpClient httpClient) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => httpClient;
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = responseFactory(request);
            return Task.FromResult(response);
        }
    }
}