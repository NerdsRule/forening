namespace Organization.Shared.Interfaces;

/// <summary>
/// Service contract for email confirmation flows.
/// </summary>
public interface IEmailConfirmationService
{
    /// <summary>
    /// Requests a new email confirmation token for the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation status.</returns>
    Task<FormResult> RequestEmailConfirmationTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms the authenticated user's email using a token.
    /// </summary>
    /// <param name="model">Request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation status.</returns>
    Task<FormResult> ConfirmEmailAsync(EmailConfirmationConfirmModel model, CancellationToken cancellationToken = default);
}