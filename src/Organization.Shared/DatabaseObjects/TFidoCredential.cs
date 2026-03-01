namespace Organization.Shared.DatabaseObjects;

/// <summary>
/// Represents a FIDO2 credential associated with an application user.
/// This class stores the cryptographic key material and metadata required for FIDO2 authentication.
/// </summary>
[Table("TFidoCredentials")]
public class TFidoCredential : TBaseTable
{
    /// <summary>
    /// Gets or sets the unique identifier of the associated application user.
    /// This is a foreign key reference to the AspNetUsers table.
    /// </summary>
    [Required(ErrorMessage = "AppUserId is required."), ForeignKey("AspNetUsers")]
    public string AppUserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the associated application user entity.
    /// </summary>
    public virtual AppUser? AppUser { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the FIDO2 credential.
    /// Maximum length is 1024 characters.
    /// </summary>
    [Required(ErrorMessage = "CredentialId is required."), MaxLength(1024)]
    public string CredentialId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the public key associated with this credential in PEM or binary format.
    /// </summary>
    [Required(ErrorMessage = "PublicKey is required.")]
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user handle associated with this credential, used for user identification during authentication.
    /// </summary>
    public string UserHandle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the signature counter value used to detect cloned authenticators and prevent replay attacks.
    /// </summary>
    public uint SignatureCounter { get; set; }

    /// <summary>
    /// Gets or sets a user-friendly name for the credential (e.g., "My Security Key").
    /// Maximum length is 100 characters.
    /// </summary>
    [MaxLength(100, ErrorMessage = "FriendlyName cannot exceed 100 characters.")]
    public string? FriendlyName { get; set; }

    /// <summary>
    /// Gets or sets the type of the credential. Defaults to "public-key".
    /// Maximum length is 32 characters.
    /// </summary>
    [MaxLength(32, ErrorMessage = "CredentialType cannot exceed 32 characters.")]
    public string CredentialType { get; set; } = "public-key";

    /// <summary>
    /// Gets or sets the Authenticator Attestation GUID (AAGUID) that identifies the type of authenticator.
    /// </summary>
    public string? AaGuid { get; set; }

    /// <summary>
    /// Gets or sets the comma-separated list of transport mechanisms supported by the authenticator (e.g., "usb,ble").
    /// Maximum length is 1024 characters.
    /// </summary>
    [MaxLength(1024, ErrorMessage = "Transports cannot exceed 1024 characters.")]
    public string? Transports { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the credential was created in UTC.
    /// Defaults to the current UTC time.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}