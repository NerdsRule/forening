
namespace Organization.Shared.Helpers;

/// <summary>
/// Helper class for managing version information of the application or components. This class can be extended to include methods for retrieving version details, comparing versions, or formatting version strings as needed.
/// </summary>
public class VersionHelper
{
    /// <summary>
    /// Api version.
    /// </summary>
    public string ApiVersion { get; init; } = GlobalShared.ApiVersion;

    /// <summary> 
    /// Blazor version.
    /// </summary>
    public string BlazorVersion { get; init; } = GlobalShared.BlazorVersion;

    /// <summary>
    /// Verify it current version matches the expected version. This can be used to ensure compatibility between frontend and backend components.
    /// </summary>
    /// <returns>True if the current version matches the expected version; otherwise, false.</returns>
    public bool IsApiVersionCompatible() => ApiVersion == GlobalShared.ApiVersion;

    /// <summary>
    /// Verify it current Blazor version matches the expected version. This can be used to ensure compatibility between frontend and backend components.
    /// </summary>
    /// <returns>True if the current Blazor version matches the expected version; otherwise, false.</returns>
    public bool IsBlazorVersionCompatible() => BlazorVersion == GlobalShared.BlazorVersion;
}
