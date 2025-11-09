


namespace Organization.Test;

public class OrganizationTesting
{

    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

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
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        
        #region Create Test Organization and add User
        var testOrgResponse = await httpClient.GetFromJsonAsync<TOrganization>("/v1/api/organization/test", cancellationToken);
        testOrgResponse.Should().NotBeNull("Test organization creation response was null.");
        testOrgResponse!.Id.Should().BeGreaterThan(0, "Test organization ID was not greater than 0.");
        // add user to the organization
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
        #endregion

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
        getAllResponse.Should().HaveCount(2, "There should be exactly two organizations after adding one.");
        getAllResponse![1].Id.Should().Be(createdOrg.Id, "The ID of the retrieved organization did not match the created one.");
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
        verifyDeleteResponse.Should().HaveCount(1, "There should be exactly one organization after deletion.");

    }
}
