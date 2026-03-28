
namespace Organization.Shared.Helpers;

/// <summary>
/// Helper class for managing version information of the application or components.
/// </summary>
public class VersionHelper
{
    /// <summary>
    /// The API version, sourced from the API assembly. Populated by the /v1/api/version endpoint.
    /// </summary>
    public string ApiVersion { get; init; } = GlobalShared.ApiVersion;

    /// <summary>
    /// The Blazor frontend version. Empty when returned by the API — the Blazor client populates
    /// this from its own assembly's AssemblyInformationalVersion after deserialization.
    /// </summary>
    public string BlazorVersion { get; set; } = string.Empty;

    /// <summary>
    /// Returns true when the API version reported by the server matches what this client was compiled against.
    /// </summary>
    public bool IsApiVersionCompatible() => ApiVersion == GlobalShared.ApiVersion;

    /// <summary>
    /// Returns true when the Blazor version matches the running assembly's version.
    /// Must be called after BlazorVersion is populated on the client.
    /// </summary>
    public bool IsBlazorVersionCompatible(string currentBlazorVersion) =>
        !string.IsNullOrEmpty(BlazorVersion) && BlazorVersion == currentBlazorVersion;

    /// <summary>
    /// Reads the AssemblyInformationalVersion (i.e. &lt;Version&gt; in the csproj) of the
    /// given assembly and strips any build-metadata suffix after '+'.
    /// </summary>
    public static string GetAssemblyVersion(System.Reflection.Assembly assembly)
    {
        var informational = System.Reflection.CustomAttributeExtensions
            .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>(assembly)
            ?.InformationalVersion ?? string.Empty;

        var plusIndex = informational.IndexOf('+');
        return plusIndex >= 0 ? informational[..plusIndex] : informational;
    }
}
