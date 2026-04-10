using System.Net;
using System.Text;
using System.Text.Json;
using Organization.Infrastructure.Services;
using Organization.Shared.Interfaces;

namespace Organization.Test;

public class EmailConfirmationServiceTests
{
    [Fact]
    public async Task RequestEmailConfirmationTokenAsync_UsesExpectedEndpointAndPostVerb()
    {
        HttpRequestMessage? capturedRequest = null;
        var responseJson = JsonSerializer.Serialize(
            new FormResult { Succeeded = true, ErrorList = ["sent"] },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var service = CreateService(request =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };
        });

        var result = await service.RequestEmailConfirmationTokenAsync(CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.RequestUri!.PathAndQuery.Should().Be("/v1/api/users/email-confirmation/request-token");
        capturedRequest.Content.Should().BeNull();
    }

    [Fact]
    public async Task ConfirmEmailAsync_PostsTokenPayloadToExpectedEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;
        var responseJson = JsonSerializer.Serialize(
            new FormResult { Succeeded = true, ErrorList = ["confirmed"] },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var service = CreateService(request =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };
        });

        var result = await service.ConfirmEmailAsync(new EmailConfirmationConfirmModel { Token = "abc123" }, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.RequestUri!.PathAndQuery.Should().Be("/v1/api/users/email-confirmation/confirm");

        var payload = await capturedRequest.Content!.ReadFromJsonAsync<EmailConfirmationConfirmModel>();
        payload.Should().NotBeNull();
        payload!.Token.Should().Be("abc123");
    }

    private static IEmailConfirmationService CreateService(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        var handler = new StubHttpMessageHandler(responseFactory);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:7375"),
        };

        var httpClientFactory = new StubHttpClientFactory(httpClient);
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<EmailConfirmationService>();

        return new EmailConfirmationService(httpClientFactory, logger);
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