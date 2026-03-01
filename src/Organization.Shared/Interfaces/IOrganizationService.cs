namespace Organization.Shared.Interfaces;

/// <summary>
/// Service contract for calling organization-related API endpoints.
/// </summary>
public interface IOrganizationService
{
    /// <summary>
    /// Get all organizations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Organizations or an API error.</returns>
    Task<(List<TOrganization>? data, FormResult? formResult)> GetOrganizationsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Get a single organization by id.
    /// </summary>
    /// <param name="id">Organization id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Organization or an API error.</returns>
    Task<(TOrganization? data, FormResult? formResult)> GetOrganizationByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Create or update an organization.
    /// </summary>
    /// <param name="organization">Organization payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated organization or an API error.</returns>
    Task<(TOrganization? data, FormResult? formResult)> AddUpdateOrganizationAsync(TOrganization organization, CancellationToken cancellationToken);

    /// <summary>
    /// Delete an organization by id.
    /// </summary>
    /// <param name="id">Organization id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result.</returns>
    Task<FormResult> DeleteOrganizationAsync(int id, CancellationToken cancellationToken);
}