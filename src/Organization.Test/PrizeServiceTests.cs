using System.Net;
using System.Text;
using System.Text.Json;
using Organization.Infrastructure.Services;
using Organization.Shared.Interfaces;

namespace Organization.Test;

public class PrizeServiceTests
{
    [Fact]
    public async Task GetPrizesByDepartmentIdAsync_WhenApiReturnsOk_ReturnsPrizeList()
    {
        // Arrange
        var expected = new List<TPrize>
        {
            new() { Id = 1, Name = "Coffee mug", CreatorUserId = "u1", DepartmentId = 42, PointsCost = 100 },
            new() { Id = 2, Name = "Gift card", CreatorUserId = "u1", DepartmentId = 42, PointsCost = 250 },
        };

        var responseJson = JsonSerializer.Serialize(expected, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var service = CreateService(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            });

        // Act
        var result = await service.GetPrizesByDepartmentIdAsync(42, CancellationToken.None);

        // Assert
        result.formResult.Should().BeNull();
        result.data.Should().NotBeNull();
        result.data!.Should().HaveCount(2);
        result.data.Select(p => p.Name).Should().Contain(["Coffee mug", "Gift card"]);
    }

    [Fact]
    public async Task GetPrizeByIdAsync_WhenApiReturnsBadRequest_ReturnsFormResult()
    {
        // Arrange
        var apiError = new FormResult { Succeeded = false, ErrorList = ["Price not found"] };
        var responseJson = JsonSerializer.Serialize(apiError, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var service = CreateService(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            });

        // Act
        var result = await service.GetPrizeByIdAsync(999, CancellationToken.None);

        // Assert
        result.data.Should().BeNull();
        result.formResult.Should().NotBeNull();
        result.formResult!.Succeeded.Should().BeFalse();
        result.formResult.ErrorList.Should().Contain("Price not found");
    }

    [Fact]
    public async Task AddUpdatePrizeAsync_WhenPrizeIsNull_ReturnsValidationError()
    {
        // Arrange
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var result = await service.AddUpdatePrizeAsync(null!, CancellationToken.None);

        // Assert
        result.data.Should().BeNull();
        result.formResult.Should().NotBeNull();
        result.formResult!.Succeeded.Should().BeFalse();
        result.formResult.ErrorList.Should().Contain("Prize cannot be null");
    }

    [Fact]
    public async Task DeletePrizeAsync_WhenApiReturnsOkAndEmptyBody_ReturnsSucceededTrue()
    {
        // Arrange
        var service = CreateService(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty),
            });

        // Act
        var result = await service.DeletePrizeAsync(5, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task GetPrizeByIdAsync_WhenHandlerThrows_ReturnsGenericErrorFormResult()
    {
        // Arrange
        var service = CreateService(_ => throw new HttpRequestException("network down"));

        // Act
        var result = await service.GetPrizeByIdAsync(10, CancellationToken.None);

        // Assert
        result.data.Should().BeNull();
        result.formResult.Should().NotBeNull();
        result.formResult!.Succeeded.Should().BeFalse();
        result.formResult.ErrorList.Should().Contain("Error retrieving prize");
    }

    private static IPrizeService CreateService(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        var handler = new StubHttpMessageHandler(responseFactory);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:7375"),
        };

        var httpClientFactory = new StubHttpClientFactory(httpClient);
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<PrizeService>();

        return new PrizeService(httpClientFactory, logger);
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
