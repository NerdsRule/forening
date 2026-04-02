namespace Organization.ApiService.Services;

/// <summary>
/// Abstraction for sending emails from the backend.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Message subject line.</param>
    /// <param name="htmlBody">HTML body content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// True when the provider accepted the send request; otherwise false.
    /// </returns>
    Task<bool> SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken);
}
