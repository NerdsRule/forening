
/// <summary>
/// Request model for renaming a WebAuthn credential.
/// </summary>
public class WebAuthCredentialRenameRequest
{
    /// <summary>
    /// The new friendly name for the credential (maximum 200 characters).
    /// </summary>
    [MaxLength(200, ErrorMessage = "Friendly name cannot exceed 200 characters.")]
    public string? FriendlyName { get; set; }
}
