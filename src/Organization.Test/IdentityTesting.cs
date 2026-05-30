
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Organization.Shared.Identity;

namespace Organization.Test;

[Collection("Integration")]
public class IdentityTesting
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(45);
    private static readonly TimeSpan StartupStepTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan DisposeStepTimeout = TimeSpan.FromSeconds(90);

    [IntegrationFact]
    public async Task IdentityTest()
    {
        // Arrange
        using var cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellationTokenSource.CancelAfter(DefaultTimeout);
        var cancellationToken = cancellationTokenSource.Token;

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Organization_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        try
        {
            await StepAsync("Start app", () => app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken), StartupStepTimeout);

            // Act
            using var httpClient = app.CreateHttpClient("apiservice");
            httpClient.Timeout = TimeSpan.FromMinutes(2);
            await StepAsync("Wait apiservice healthy", () => app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken), StartupStepTimeout);

            // Add an organization
            var testOrgResponse = await StepAsync("Create test org", () => httpClient.GetFromJsonAsync<TOrganization>("/v1/api/organization/test", cancellationToken));
            testOrgResponse.Should().NotBeNull("Test organization creation response was null.");
            testOrgResponse!.Id.Should().BeGreaterThan(0, "Test organization ID was not greater than 0.");

            // Test GetInfo before user registration where unauthenticated access is expected
            var getInfoHttpResponse = await StepAsync("Get anonymous user info", () => httpClient.GetAsync("/v1/api/users/info", cancellationToken));
            getInfoHttpResponse.IsSuccessStatusCode.Should().BeFalse("GetInfo before registration should not succeed for an unauthenticated user.");

            var registerResponse = await StepAsync("Register test user", () => httpClient.GetFromJsonAsync<UserModel>($"/v1/api/users/test/{testOrgResponse.Id}", cancellationToken));
            registerResponse.Should().NotBeNull("Register response was null.");
            registerResponse!.UserName.Should().Be("testuser@example.com", "Registered user email was not correct.");

            // Login user
            var loginModel = new LoginModel
            {
                Email = registerResponse!.UserName,
                Password = "TestPassword123!"
            };
            var loginResponse = await StepAsync("Login #1", () => httpClient.PostAsJsonAsync("/v1/api/users/login", loginModel, cancellationToken));
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Login response status code was not OK.");

            // Logout user
            const string Empty = "{}";
            var emptyContent = new StringContent(Empty, Encoding.UTF8, "application/json");
            var logoutResponse = await StepAsync("Logout #1", () => httpClient.PostAsync("/v1/api/users/logout", emptyContent, cancellationToken));
            logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Logout response status code was not OK.");

            // Login again to test role assignment
            loginResponse = await StepAsync("Login #2", () => httpClient.PostAsJsonAsync("/v1/api/users/login", loginModel, cancellationToken));
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Second login response status code was not OK.");

            // Logout user
            logoutResponse = await StepAsync("Logout #2", () => httpClient.PostAsync("/v1/api/users/logout", emptyContent, cancellationToken));
            logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Logout response status code was not OK.");

            // Login again to test role assignment
            loginResponse = await StepAsync("Login #3", () => httpClient.PostAsJsonAsync("/v1/api/users/login", loginModel, cancellationToken));
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Second login response status code was not OK.");

            // Verify authenticated user info with current API contract
            var authenticatedUserInfo = await StepAsync("Get authenticated user info", () => httpClient.GetFromJsonAsync<UserModel>("/v1/api/users/info", cancellationToken));
            authenticatedUserInfo.Should().NotBeNull("Authenticated user info response was null.");
            authenticatedUserInfo!.UserName.Should().Be(registerResponse.UserName, "Authenticated user username did not match the registered test user.");
        }
        finally
        {
            await StepAsync("Dispose app", () => app.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken), DisposeStepTimeout);
        }
    }

    private async Task<T> StepAsync<T>(string name, Func<Task<T>> action, TimeSpan? timeout = null)
    {
        var started = DateTimeOffset.UtcNow;
        Console.WriteLine($"[{started:O}] START {name}");
        var result = await action().WaitAsync(timeout ?? StepTimeout, TestContext.Current.CancellationToken);
        var finished = DateTimeOffset.UtcNow;
        Console.WriteLine($"[{finished:O}] END {name} (+{(finished - started).TotalMilliseconds:0} ms)");
        return result;
    }

    private async Task StepAsync(string name, Func<Task> action, TimeSpan? timeout = null)
    {
        var started = DateTimeOffset.UtcNow;
        Console.WriteLine($"[{started:O}] START {name}");
        await action().WaitAsync(timeout ?? StepTimeout, TestContext.Current.CancellationToken);
        var finished = DateTimeOffset.UtcNow;
        Console.WriteLine($"[{finished:O}] END {name} (+{(finished - started).TotalMilliseconds:0} ms)");
    }
}
