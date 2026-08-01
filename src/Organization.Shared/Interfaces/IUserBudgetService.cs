namespace Organization.Shared.Interfaces;

/// <summary>
/// Service contract for calling user budget API endpoints.
/// </summary>
public interface IUserBudgetService
{
    /// <summary>
    /// Get the budget entries for the authenticated user in a department.
    /// </summary>
    /// <param name="departmentId">Department identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The budget entries, or a form result if the request failed.</returns>
    Task<(List<TUserBudget>? data, FormResult? formResult)> GetMyBudgetAsync(int departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Get the budget entries for a specific user in a department.
    /// </summary>
    /// <param name="departmentId">Department identifier.</param>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The budget entries, or a form result if the request failed.</returns>
    Task<(List<TUserBudget>? data, FormResult? formResult)> GetUserBudgetAsync(int departmentId, string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Get all budget entries for a department.
    /// </summary>
    /// <param name="departmentId">Department identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The department budget entries, or a form result if the request failed.</returns>
    Task<(List<TUserBudget>? data, FormResult? formResult)> GetDepartmentBudgetsAsync(int departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Create or update a budget entry.
    /// </summary>
    /// <param name="budget">The budget entry to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved budget entry, or a form result if the operation failed.</returns>
    Task<(TUserBudget? data, FormResult? formResult)> AddUpdateBudgetAsync(TUserBudget budget, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a budget entry by identifier.
    /// </summary>
    /// <param name="id">Budget entry identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the delete operation.</returns>
    Task<FormResult> DeleteBudgetAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Get all distinct budget tags used in a department.
    /// </summary>
    /// <param name="departmentId">Department identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tag suggestions, or a form result if the request failed.</returns>
    Task<(List<string>? data, FormResult? formResult)> GetDistinctBudgetTagsByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Get description suggestions for budget entries in a department.
    /// </summary>
    /// <param name="departmentId">Department identifier.</param>
    /// <param name="query">Optional search text to filter suggestions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching description suggestions, or a form result if the request failed.</returns>
    Task<(List<string>? data, FormResult? formResult)> GetBudgetDescriptionSuggestionsAsync(int departmentId, string query, CancellationToken cancellationToken);
}