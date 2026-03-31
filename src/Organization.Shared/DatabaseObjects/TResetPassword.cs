
namespace Organization.Shared.DatabaseObjects;

/// <summary>
/// Represents the TResetPassword table in the database, which stores information related to password reset requests.
/// </summary>
[Table("TResetPasswords")]
public class TResetPassword : TBaseTable
{
	/// <summary>
	/// Maximum number of reset-mail requests allowed before the user is blocked.
	/// </summary>
	public const int MaxResetRequestsBeforeBlock = 2;

	/// <summary>
	/// Foreign key to the user that requested password reset mails.
	/// </summary>
	[Required, ForeignKey("AspNetUsers")]
	public string AppUserId { get; set; } = string.Empty;

	/// <summary>
	/// Navigation property for the linked user.
	/// </summary>
	public virtual AppUser? AppUser { get; set; }

	/// <summary>
	/// Current number of password reset-mail requests submitted.
	/// </summary>
	public int ResetRequestCount { get; set; }

	/// <summary>
	/// Indicates whether reset-mail sending is blocked for the user.
	/// </summary>
	public bool IsResetMailBlocked { get; set; }

	/// <summary>
	/// UTC timestamp for the latest password reset-mail request.
	/// </summary>
	public DateTimeOffset? LastResetRequestAt { get; set; }

	/// <summary>
	/// Reset token generated for the latest password reset request.
	/// This token must be submitted with the new password.
	/// </summary>
	[MaxLength(4000)]
	public string? ResetToken { get; set; }

	/// <summary>
	/// UTC timestamp for when the current reset token was created.
	/// </summary>
	public DateTimeOffset? ResetTokenCreatedAt { get; set; }

	/// <summary>
	/// UTC timestamp for when reset-mail sending was blocked.
	/// </summary>
	public DateTimeOffset? ResetMailBlockedAt { get; set; }

	/// <summary>
	/// True when a reset mail can be sent according to current state and request count.
	/// </summary>
	[NotMapped]
	public bool CanSendResetMail => !IsResetMailBlocked && ResetRequestCount < MaxResetRequestsBeforeBlock;

	/// <summary>
	/// Registers a reset-mail request and blocks further requests after the second one.
	/// </summary>
	/// <param name="requestedAtUtc">The UTC timestamp for the incoming request.</param>
	/// <param name="resetToken">The reset token generated for this request.</param>
	public void RegisterResetRequest(DateTimeOffset requestedAtUtc, string resetToken)
	{
		LastResetRequestAt = requestedAtUtc;
		ResetToken = resetToken;
		ResetTokenCreatedAt = requestedAtUtc;
		ResetRequestCount++;

		if (ResetRequestCount >= MaxResetRequestsBeforeBlock)
		{
			IsResetMailBlocked = true;
			ResetMailBlockedAt = requestedAtUtc;
		}
	}

	/// <summary>
	/// Checks whether the submitted token matches the currently stored reset token.
	/// </summary>
	/// <param name="token">Submitted reset token.</param>
	/// <returns>True when token matches; otherwise false.</returns>
	public bool IsMatchingResetToken(string? token)
	{
		return !string.IsNullOrWhiteSpace(token) && string.Equals(ResetToken, token, StringComparison.Ordinal);
	}


}
