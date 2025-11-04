
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

        var response = await httpClient.GetFromJsonAsync<List<TOrganization>>("/v1/api/organization/all", cancellationToken);
        response.Should().NotBeNull("Response content was null.");
        response.Should().BeOfType<List<TOrganization>>("Response content was not of expected type List<TOrganization>.");
        response.Should().BeEmpty("Response content was expected to be empty as no organizations have been added yet.");

        // Add an organization
        var newOrg = new TOrganization
        {
            Name = "Test Organization",
        };
        var putResponse = await httpClient.PutAsJsonAsync("/v1/api/organization", newOrg, cancellationToken);
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK, "PUT response status code was not OK.");
        var createdOrg = await putResponse.Content.ReadFromJsonAsync<TOrganization>(cancellationToken);
        createdOrg.Should().NotBeNull("Created organization was null.");
        createdOrg!.Id.Should().BeGreaterThan(0, "Created organization ID was not greater than 0.");
        createdOrg.Name.Should().Be(newOrg.Name, "Created organization name did not match.");
        createdOrg.IsActive.Should().BeTrue("Created organization IsActive was not true by default.");

        // Add user
        var registerModel = new RegisterModel
        {
            Email = "test@example.com",
            Password = "P@ssw0rd",
            ConfirmPassword = "P@ssw0rd",
            OrganizationId = createdOrg.Id
        };
        var registerResponse = await httpClient.PostAsJsonAsync("/v1/api/users/register", registerModel, cancellationToken);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Register response status code was not OK.");
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<UserModel>(cancellationToken);
        registerResult.Should().NotBeNull("Register result was null.");
        
        // Login user
        var loginModel = new LoginModel
        {
            Email = registerModel.Email,
            Password = registerModel.Password
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
        
        // During startup all roles are added here
        var roleContent = new string[] { OrganizationRolesEnum.EnterpriseAdmin.ToString() };
        var roleResponse = await httpClient.PostAsJsonAsync("/v1/api/roles", roleContent);
        roleResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Add role response status code was not OK.");

        // Add role to user. Will only work for first user.
        var userWithRolesResponse = await httpClient.PostAsJsonAsync("/v1/api/users/" + registerResult?.Id + "/roles", roleContent);
        userWithRolesResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Add role to user response status code was not OK.");

        // Logout user
        logoutResponse = await httpClient.PostAsync("/v1/api/users/logout", emptyContent);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Logout response status code was not OK.");

        // Login again to test role assignment
        loginResponse = await httpClient.PostAsJsonAsync("/v1/api/users/login", loginModel, cancellationToken);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Second login response status code was not OK.");

        // Get all roles and verify
        var getRolesResponse = await httpClient.GetFromJsonAsync<List<string>>("/v1/api/roles/all");
        getRolesResponse.Should().NotBeNull("Get roles response was null.");
        getRolesResponse.Should().Contain(OrganizationRolesEnum.EnterpriseAdmin.ToString(), "Get roles response did not contain the added role.");

        //var applicationCookie = loginResponse.Headers.GetValues("Set-Cookie").FirstOrDefault(c => c.StartsWith(".AspNetCore.Identity.Application="));
        //applicationCookie.Should().NotBeNullOrEmpty("Application cookie was not set.");
        //var cookieValue = applicationCookie.Split('=')[1].Split(';')[0];
        //cookieValue = Uri.UnescapeDataString(cookieValue);

        // Get the data protection provider from the app services
        
        //var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResultModel>(cancellationToken);
        //loginResult.Should().NotBeNull("Login result was null.");
        //loginResult!.Token.Should().NotBeNullOrEmpty("Login token was null or empty.");
    }
}
