
namespace Organization.Test.Tests;

public class IntegrationTest1
{
    [Fact]
    public async Task GetApiVersion_ReturnsVersionInfo()
    {
        var apiAssembly = typeof(Organization.ApiService.V1.VersionEndpoint).Assembly;
        var apiVersion = Organization.Shared.Helpers.VersionHelper.GetAssemblyVersion(apiAssembly);

        apiVersion.Should().NotBeNullOrWhiteSpace();
    }

}
