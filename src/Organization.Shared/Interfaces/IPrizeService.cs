namespace Organization.Shared.Interfaces;

/// <summary>
/// Service contract for calling prize-related API endpoints.
/// </summary>
public interface IPrizeService
{
    /// <summary>
    /// Retrieves prizes for a department.
    /// </summary>
    /// <param name="departmentId">Department id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Prizes or an API error.</returns>
    Task<(List<TPrize>? data, FormResult? formResult)> GetPrizesByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a prize by id.
    /// </summary>
    /// <param name="id">Prize id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Prize or an API error.</returns>
    Task<(TPrize? data, FormResult? formResult)> GetPrizeByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates a prize.
    /// </summary>
    /// <param name="prize">Prize payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated prize or an API error.</returns>
    Task<(TPrize? data, FormResult? formResult)> AddUpdatePrizeAsync(TPrize prize, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a prize by id.
    /// </summary>
    /// <param name="id">Prize id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result.</returns>
    Task<FormResult> DeletePrizeAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves awarded, redeemed and balance points for a user.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User points totals or an API error.</returns>
    Task<(UserPointsBalanceModel? data, FormResult? formResult)> GetPointsBalanceByUserIdAsync(string userId, CancellationToken cancellationToken);
}
