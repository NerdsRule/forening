
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Organization.Shared.Identity;

namespace Organization.Test;

public class IdentityTesting
{
private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);
[Fact]
    public async Task IdentityTest()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var timeout = TimeSpan.FromMinutes(5);
        var cancellationTokenSource = new CancellationTokenSource(timeout);
        cancellationToken = cancellationTokenSource.Token;
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

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        using var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Add an organization
        var testOrgResponse = await httpClient.GetFromJsonAsync<TOrganization>("/v1/api/organization/test", cancellationToken);
        testOrgResponse.Should().NotBeNull("Test organization creation response was null.");
        testOrgResponse!.Id.Should().BeGreaterThan(0, "Test organization ID was not greater than 0.");

        // Test GetInfo before user registration where 401 is expected
        try
        {
            var getInfoResponse = await httpClient.GetFromJsonAsync<UserModel?>($"/v1/api/users/info", cancellationToken);
            getInfoResponse.Should().BeNull("GetInfo response before registration should be null (unauthenticated).");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Expected exception for unauthenticated request
        }
        
        var registerResponse = await httpClient.GetFromJsonAsync<UserModel>($"/v1/api/users/test/{testOrgResponse.Id}", cancellationToken);
        registerResponse.Should().NotBeNull("Register response was null.");
        registerResponse!.UserName.Should().Be("testuser@example.com", "Registered user email was not correct.");

        // Login user
        var loginModel = new LoginModel
        {
            Email = registerResponse!.UserName,
            Password = "TestPassword123!"
        };
        var loginResponse = await httpClient.PostAsJsonAsync("/v1/api/users/login", loginModel, cancellationToken);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Login response status code was not OK.");

        // Logout user
        const string Empty = "{}";
        var emptyContent = new StringContent(Empty, Encoding.UTF8, "application/json");
        var logoutResponse = await httpClient.PostAsync("/v1/api/users/logout", emptyContent);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Logout response status code was not OK.");

        // Login again to test role assignment
        loginResponse = await httpClient.PostAsJsonAsync("/v1/api/users/login", loginModel, cancellationToken);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Second login response status code was not OK.");
        
        // Logout user
        logoutResponse = await httpClient.PostAsync("/v1/api/users/logout", emptyContent);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Logout response status code was not OK.");

        // Login again to test role assignment
        loginResponse = await httpClient.PostAsJsonAsync("/v1/api/users/login", loginModel, cancellationToken);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Second login response status code was not OK.");

        // Get all roles and verify
        var getRolesResponse = await httpClient.GetFromJsonAsync<List<string>>("/v1/api/roles/all");
        getRolesResponse.Should().NotBeNull("Get roles response was null.");
        getRolesResponse.Should().Contain(RolesEnum.EnterpriseAdmin.ToString(), "Get roles response did not contain the added role.");

        // Add a role to user and verify
        var additionalRole = RolesEnum.DepartmentAdmin.ToString();
        var addAdditionalRoleResponse = await httpClient.PostAsJsonAsync("/v1/api/users/" + registerResponse?.Id + "/roles", new string[] { additionalRole });
        addAdditionalRoleResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Add additional role to user response status code was not OK.");
        //Logout user
        logoutResponse = await httpClient.PostAsync("/v1/api/users/logout", emptyContent);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Logout response status code was not OK.");
        // Login again to test role assignment
        loginResponse = await httpClient.PostAsJsonAsync("/v1/api/users/login", loginModel, cancellationToken);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Third login response status code was not OK.");
        var getUserRolesResponse = await httpClient.GetFromJsonAsync<List<string>>($"/v1/api/users/{registerResponse?.Id}/roles");
        getUserRolesResponse.Should().NotBeNull("Get user roles response was null.");
        getUserRolesResponse.Should().Contain(additionalRole, "Get user roles response did not contain the additional role.");
        getUserRolesResponse.Should().Contain(RolesEnum.EnterpriseAdmin.ToString(), "Get user roles response did not contain the enterprise admin role.");

    }
}
