
namespace Organization.Shared.Identity;

/// <summary>
/// Request model for updating the friendly name of a WebAuthn credential.
/// </summary>
public class WebAuthnUpdateFriendlyNameRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the credential to update.
    /// </summary>
    [Required(ErrorMessage = "FriendlyName is required.")]
    [MaxLength(100, ErrorMessage = "FriendlyName cannot exceed 100 characters.")]
    public string FriendlyName { get; set; } = string.Empty;
}
