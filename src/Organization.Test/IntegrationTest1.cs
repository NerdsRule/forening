using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Organization.Test.Tests;

public class IntegrationTest1
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    // Instructions:
    // 1. Add a project reference to the target AppHost project, e.g.:
    //
    //    <ItemGroup>
    //        <ProjectReference Include="../MyAspireApp.AppHost/MyAspireApp.AppHost.csproj" />
    //    </ItemGroup>
    //
    // 2. Uncomment the following example test and update 'Projects.MyAspireApp_AppHost' to match your AppHost project:
    //
    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        //var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MyAspireApp_AppHost>(cancellationToken);
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Organization_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
            // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        using var httpClient = app.CreateHttpClient("webfrontend");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("webfrontend", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        using var response = await httpClient.GetAsync("/", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetApiService_GetWeather_ReturnsOkAndJson()
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

        using var response = await httpClient.GetAsync("/v1/weatherforecast", cancellationToken);

        // Assert - basic checks: OK and JSON payload (array or object)
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        content.Should().NotBeNullOrWhiteSpace("Response content was empty.");

        JsonDocument doc = JsonDocument.Parse(content);
        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array || doc.RootElement.ValueKind == JsonValueKind.Object, "Expected JSON array or object from GetWeather.");
    }

    [Fact]
    public async Task GetApiService_GetOrganization()
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
        // Get all organizations again
        var getAllResponse = await httpClient.GetFromJsonAsync<List<TOrganization>>("/v1/api/organization/all", cancellationToken);
        getAllResponse.Should().NotBeNull("Get all response content was null.");
        getAllResponse.Should().HaveCount(1, "There should be exactly one organization after adding one.");
        getAllResponse![0].Id.Should().Be(createdOrg.Id, "The ID of the retrieved organization did not match the created one.");
        // Get the organization by ID
        var getByIdResponse = await httpClient.GetFromJsonAsync<TOrganization>($"/v1/api/organization/{createdOrg.Id}", cancellationToken);
        getByIdResponse.Should().NotBeNull("Get by ID response content was null.");
        getByIdResponse!.Id.Should().Be(createdOrg.Id, "The ID of the retrieved organization did not match the created one.");
        getByIdResponse.Name.Should().Be(createdOrg.Name, "The name of the retrieved organization did not match the created one.");
        // Clean up - delete the organization
        var deleteResponse = await httpClient.DeleteAsync($"/v1/api/organization/{createdOrg.Id}", cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Delete response status code was not OK.");
        // Verify deletion
        var verifyDeleteResponse = await httpClient.GetFromJsonAsync<List<TOrganization>>("/v1/api/organization/all", cancellationToken);
        verifyDeleteResponse.Should().NotBeNull("Verify delete response content was null.");
        verifyDeleteResponse.Should().BeEmpty("Verify delete response content was not empty.");

    }
}
