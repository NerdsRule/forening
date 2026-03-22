namespace Organization.Shared.Interfaces;

/// <summary>
/// Service contract for calling version-related API endpoints.
/// </summary>
public interface IVersionService
{
    /// <summary>
    /// Retrieves API and Blazor version information from the backend.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Version information or an API error.</returns>
    Task<(VersionHelper? data, FormResult? formResult)> GetVersionAsync(CancellationToken cancellationToken);
}