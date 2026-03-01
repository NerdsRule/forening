namespace Organization.Shared.Interfaces;

/// <summary>
/// Service contract for calling department-related API endpoints.
/// </summary>
public interface IDepartmentService
{
    /// <summary>
    /// Get departments by organization id for a specific user.
    /// </summary>
    /// <param name="userId">The user id used by the endpoint for access checks.</param>
    /// <param name="organizationId">Organization id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Departments or an API error.</returns>
    Task<(List<TDepartment>? data, FormResult? formResult)> GetDepartmentsByOrganizationIdAsync(string userId, int organizationId, CancellationToken cancellationToken);

    /// <summary>
    /// Create or update a department.
    /// </summary>
    /// <param name="department">Department payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated department or an API error.</returns>
    Task<(TDepartment? data, FormResult? formResult)> AddUpdateDepartmentAsync(TDepartment department, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a department by id.
    /// </summary>
    /// <param name="id">Department id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result.</returns>
    Task<FormResult> DeleteDepartmentAsync(int id, CancellationToken cancellationToken);
}