
namespace Organization.Shared.Identity;

/// <summary>
/// Represents the request payload for initiating a WebAuthn passkey login process.
/// This class contains the email address of the user attempting to log in with a passkey.
/// The email is required to identify the user and retrieve their registered passkeys for authentication.
/// </summary>
public class WebAuthnBeginPasskeyLoginRequest
{
    /// <summary>
    ///     Gets or sets the email address of the user attempting to log in with a passkey.
     ///     This is required to identify the user and retrieve their registered passkeys for authentication.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;
}
