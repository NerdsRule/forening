namespace Organization.ApiService.Services;
using Resend;

/// <summary>
/// Sends emails through the Resend HTTP API.
/// </summary>
/// <remarks>
/// Uses the official Resend .NET SDK client (<see cref="IResend"/>).
/// Reads sender configuration from <c>Resend:FromEmail</c> and <c>Resend:FromName</c>,
/// with environment variable fallback for sender email.
/// </remarks>
public class ResendEmailSender : IEmailSender
{
    private readonly IResend _resend;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResendEmailSender> _logger;
    private string? FromEmail => Environment.GetEnvironmentVariable("RESEND_FROM_EMAIL") ?? _configuration["Resend:FromEmail"];
    private string? FromName => Environment.GetEnvironmentVariable("RESEND_FROM_NAME") ?? _configuration["Resend:FromName"];

    /// <summary>
    /// Initializes a new instance of the <see cref="ResendEmailSender"/> class.
    /// </summary>
    /// <param name="resend">Resend SDK client.</param>
    /// <param name="configuration">Application configuration source.</param>
    /// <param name="logger">Logger instance.</param>
    public ResendEmailSender(IResend resend, IConfiguration configuration, ILogger<ResendEmailSender> logger)
    {
        _resend = resend;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Sends an email using Resend.
    /// </summary>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Message subject line.</param>
    /// <param name="htmlBody">HTML body content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// True when Resend responds with a successful status code; otherwise false.
    /// </returns>
    public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(FromEmail))
        {
            _logger.LogWarning("Resend email not sent because configuration is missing (FromEmail).");
            return false;
        }

        var from = string.IsNullOrWhiteSpace(FromName)
            ? FromEmail
            : $"{FromName} <{FromEmail}>";

        var message = new EmailMessage
        {
            From = from,
            Subject = subject,
            HtmlBody = htmlBody
        };
        message.To.Add(toEmail);

        try
        {
            await _resend.EmailSendAsync(message, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Resend email send failed for recipient {ToEmail}.", toEmail);
            return false;
        }
    }
}
