using System.Net;
using System.Text;
using System.Text.Json;
using Organization.Infrastructure.Services;
using Organization.Shared.Interfaces;

namespace Organization.Test;

public class OrganizationServiceTests
{
    [Fact]
    public async Task GetOrganizationsAsync_WhenApiReturnsOk_ReturnsOrganizationList()
    {
        // Ensure success responses are deserialized into organization data.
        var expected = new List<TOrganization>
        {
            new() { Id = 1, Name = "Org A" },
            new() { Id = 2, Name = "Org B" },
        };
        var responseJson = JsonSerializer.Serialize(expected, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var service = CreateService(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            });

        var (data, formResult) = await service.GetOrganizationsAsync(TestContext.Current.CancellationToken);

        formResult.Should().BeNull();
        data.Should().NotBeNull();
        data!.Select(x => x.Name).Should().Contain(["Org A", "Org B"]);
    }

    [Fact]
    public async Task AddUpdateOrganizationAsync_WhenOrganizationIsNull_ReturnsValidationError()
    {
        // Null input should be rejected before any HTTP call is attempted.
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK));

        var (data, formResult) = await service.AddUpdateOrganizationAsync(null!, TestContext.Current.CancellationToken);

        data.Should().BeNull();
        formResult.Should().NotBeNull();
        formResult!.Succeeded.Should().BeFalse();
        formResult.ErrorList.Should().Contain("Organization cannot be null");
    }

    [Fact]
    public async Task DeleteOrganizationAsync_WhenApiReturnsOkAndNoBody_ReturnsSucceededTrue()
    {
        // Empty-body successful delete should still be treated as success.
        var service = CreateService(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty),
            });

        var result = await service.DeleteOrganizationAsync(7, TestContext.Current.CancellationToken);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task GetOrganizationByIdAsync_WhenHandlerThrows_ReturnsGenericErrorFormResult()
    {
        // Transport exceptions should be translated into stable fallback errors.
        var service = CreateService(_ => throw new HttpRequestException("network down"));

        var (data, formResult) = await service.GetOrganizationByIdAsync(123, TestContext.Current.CancellationToken);

        data.Should().BeNull();
        formResult.Should().NotBeNull();
        formResult!.Succeeded.Should().BeFalse();
        formResult.ErrorList.Should().Contain("Error retrieving organization");
    }

    private static IOrganizationService CreateService(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        var handler = new StubHttpMessageHandler(responseFactory);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:7375"),
        };

        var httpClientFactory = new StubHttpClientFactory(httpClient);
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<OrganizationService>();

        return new OrganizationService(httpClientFactory, logger);
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