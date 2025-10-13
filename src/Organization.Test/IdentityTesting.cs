
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
        
    }
}
