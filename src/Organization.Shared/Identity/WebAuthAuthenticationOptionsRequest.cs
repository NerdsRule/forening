
namespace Organization.Shared.Identity;

/// <summary>
/// Request model for initiating WebAuthn authentication options.
/// </summary>
public class WebAuthAuthenticationOptionsRequest
{
    /// <summary>
    /// The email address of the user initiating authentication.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    public string Email { get; set; } = string.Empty;
}
