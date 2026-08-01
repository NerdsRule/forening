using System.Net;
using System.Text;
using System.Text.Json;
using Organization.Infrastructure.Services;
using Organization.Shared.Interfaces;

namespace Organization.Test;

public class UserBudgetServiceTests
{
    [Fact]
    public async Task GetMyBudgetAsync_WhenApiReturnsOk_ReturnsBudgetList()
    {
        var expected = new List<TUserBudget>
        {
            new() { Id = 1, AppUserId = "u1", DepartmentId = 42, Amount = 100m, Description = "Added" },
            new() { Id = 2, AppUserId = "u1", DepartmentId = 42, Amount = -25m, Description = "Spent" },
        };
        var responseJson = JsonSerializer.Serialize(expected, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
        });

        var result = await service.GetMyBudgetAsync(42, CancellationToken.None);

        result.formResult.Should().BeNull();
        result.data.Should().NotBeNull();
        result.data!.Should().HaveCount(2);
        result.data.Sum(budget => budget.Amount).Should().Be(75m);
    }

    [Fact]
    public async Task GetDepartmentBudgetsAsync_WhenApiReturnsForbidden_ReturnsFormResult()
    {
        var apiError = new FormResult { Succeeded = false, ErrorList = ["Forbidden"] };
        var responseJson = JsonSerializer.Serialize(apiError, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
        });

        var result = await service.GetDepartmentBudgetsAsync(42, CancellationToken.None);

        result.data.Should().BeNull();
        result.formResult.Should().NotBeNull();
        result.formResult!.Succeeded.Should().BeFalse();
        result.formResult.ErrorList.Should().Contain("Forbidden");
    }

    [Fact]
    public async Task AddUpdateBudgetAsync_WhenBudgetIsNull_ReturnsValidationError()
    {
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK));

        var result = await service.AddUpdateBudgetAsync(null!, CancellationToken.None);

        result.data.Should().BeNull();
        result.formResult.Should().NotBeNull();
        result.formResult!.Succeeded.Should().BeFalse();
        result.formResult.ErrorList.Should().Contain("Budget entry cannot be null");
    }

    [Fact]
    public async Task DeleteBudgetAsync_WhenApiReturnsOkAndEmptyBody_ReturnsSucceededTrue()
    {
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty),
        });

        var result = await service.DeleteBudgetAsync(5, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task GetDistinctBudgetTagsByDepartmentIdAsync_WhenApiReturnsOk_ReturnsTags()
    {
        var expected = new List<string> { "food", "travel" };
        var responseJson = JsonSerializer.Serialize(expected, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
        });

        var result = await service.GetDistinctBudgetTagsByDepartmentIdAsync(42, CancellationToken.None);

        result.formResult.Should().BeNull();
        result.data.Should().NotBeNull();
        result.data.Should().Contain(["food", "travel"]);
    }

    [Fact]
    public async Task GetBudgetDescriptionSuggestionsAsync_WhenApiReturnsOk_ReturnsSuggestions()
    {
        var expected = new List<string> { "Bus ticket", "Bus card" };
        var responseJson = JsonSerializer.Serialize(expected, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var service = CreateService(request =>
        {
            request.RequestUri!.Query.Should().Contain("query=bus");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };
        });

        var result = await service.GetBudgetDescriptionSuggestionsAsync(42, "bus", CancellationToken.None);

        result.formResult.Should().BeNull();
        result.data.Should().NotBeNull();
        result.data.Should().Contain(["Bus ticket", "Bus card"]);
    }

    private static IUserBudgetService CreateService(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        var handler = new StubHttpMessageHandler(responseFactory);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:7375"),
        };

        var httpClientFactory = new StubHttpClientFactory(httpClient);
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<UserBudgetService>();

        return new UserBudgetService(httpClientFactory, logger);
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