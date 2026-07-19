using System.Reflection;
using Organization.Shared.Helpers;

namespace Organization.Test;

public class AllProjectsSmokeTests
{
    public static TheoryData<string> ProjectAssemblyNames =>
    [
        "Organization.ApiService",
        "Organization.AppHost",
        "Organization.Blazor",
        "Organization.Core",
        "Organization.Infrastructure",
        "Organization.ServiceDefaults",
        "Organization.Shared"
    ];

    [Theory]
    [MemberData(nameof(ProjectAssemblyNames))]
    public void ProjectAssembly_CanBeLoaded(string assemblyName)
    {
        // This verifies the test project references remain valid for every solution project.
        var assembly = TryGetLoadedAssembly(assemblyName) ?? Assembly.Load(assemblyName);

        assembly.Should().NotBeNull();
        assembly.GetName().Name.Should().Be(assemblyName);
    }

    [Theory]
    [MemberData(nameof(ProjectAssemblyNames))]
    public void ProjectAssembly_HasInformationalVersion(string assemblyName)
    {
        // Version metadata is used by runtime compatibility checks and release diagnostics.
        var assembly = TryGetLoadedAssembly(assemblyName) ?? Assembly.Load(assemblyName);

        var version = VersionHelper.GetAssemblyVersion(assembly);

        version.Should().NotBeNullOrWhiteSpace();
    }

    private static Assembly? TryGetLoadedAssembly(string assemblyName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(x => string.Equals(x.GetName().Name, assemblyName, StringComparison.Ordinal));
    }
}