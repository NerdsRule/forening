namespace Organization.Shared.Identity;

/// <summary>
/// Model representing a FIDO2 credential for API responses, containing only non-sensitive information suitable for client consumption.
/// This model is used to return credential metadata without exposing sensitive key material.
/// </summary>
public class WebAuthnCredentialModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the credential.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user-friendly name for the credential (e.g., "My Security Key").
    /// </summary>
    public string? FriendlyName { get; set; }

    /// <summary>
    /// Gets or sets the type of the credential (e.g., "public-key").
    /// </summary>
    public string CredentialType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Authenticator Attestation GUID (AAGUID) that identifies the type of authenticator.
    /// </summary>
    public string Aaguid { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hint for the credential, which may include information about the authenticator or its usage.
    /// </summary>
    public string CredentialHint { get; set; } = string.Empty;


    /// <summary>
    /// Gets or sets the fingerprint of the credential, which can be used to uniquely identify the credential without exposing sensitive information.
    /// </summary>
    public string Fingerprint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the comma-separated list of transport mechanisms supported by the authenticator (e.g., "usb,ble").
    /// </summary>  
    public string? Transports { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the credential was created, which can be used for sorting or display purposes.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}