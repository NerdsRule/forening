
namespace Organization.Shared.Identity;

/// <summary>
/// Model representing a stored WebAuthn credential.
/// </summary>
public class WebAuthCredentialModel
{
    /// <summary>
    /// The unique identifier for the credential.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The credential identifier.
    /// </summary>
    public string CredentialId { get; set; } = string.Empty;

    /// <summary>
    /// Optional user-provided friendly name for the credential.
    /// </summary>
    public string? FriendlyName { get; set; }

    /// <summary>
    /// The UTC timestamp when the credential was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// The UTC timestamp when the credential was last used for authentication.
    /// </summary>
    public DateTime? LastUsedAtUtc { get; set; }
}
