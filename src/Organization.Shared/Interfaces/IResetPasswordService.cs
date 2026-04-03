namespace Organization.Shared.Interfaces;

/// <summary>
/// Service contract for password reset flows.
/// </summary>
public interface IResetPasswordService
{
    /// <summary>
    /// Starts a self-service password reset flow for a user email.
    /// </summary>
    /// <param name="model">Request payload.</param>
    /// <returns>Operation status.</returns>
    public Task<FormResult> RequestPasswordResetAsync(RequestPasswordResetModel model);

    /// <summary>
    /// Completes a self-service password reset using a reset token.
    /// </summary>
    /// <param name="model">Request payload.</param>
    /// <returns>Operation status.</returns>
    public Task<FormResult> ResetOwnPasswordAsync(SelfResetPasswordModel model);

    /// <summary>
    /// Resets another user's password as an authorized administrator.
    /// </summary>
    /// <param name="model">Request payload.</param>
    /// <returns>Operation status.</returns>
    public Task<FormResult> ResetPasswordAsync(ResetPasswordModel model);

    /// <summary>
    /// Returns all TResetPassword rows for users in the caller's organization(s). Requires EnterpriseAdmin or OrganizationAdmin.
    /// </summary>
    /// <param name="organizationId">The Id of the organization.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of reset password rows, or null on failure.</returns>
    public Task<(List<TResetPassword>? data, FormResult? result)> GetResetRequestsAsync(int organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a password reset request row by its Id. Requires EnterpriseAdmin or OrganizationAdmin.
    /// </summary>
    /// <param name="organizationId">The Id of the organization.</param>
    /// <param name="id">The Id of the reset request to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation status.</returns>
    public Task<FormResult> DeleteResetRequestAsync(int organizationId, int id, CancellationToken cancellationToken = default);
}