
namespace Organization.Shared.Identity;

/// <summary>
/// Request model for completing a WebAuthn registration ceremony.
/// </summary>
public class WebAuthRegisterCompleteRequest
{
    /// <summary>
    /// The credential identifier returned from the authenticator.
    /// </summary>
    [Required(ErrorMessage = "Credential ID is required.")]
    public string CredentialId { get; set; } = string.Empty;

    /// <summary>
    /// The client data JSON containing the challenge and origin information.
    /// </summary>
    [Required(ErrorMessage = "Client data JSON is required.")]
    public string ClientDataJson { get; set; } = string.Empty;

    /// <summary>
    /// The public key in SPKI format.
    /// </summary>
    [Required(ErrorMessage = "Public key SPKI is required.")]
    public string PublicKeySpki { get; set; } = string.Empty;

    /// <summary>
    /// The public key algorithm identifier (default: -7 for ES256).
    /// </summary>
    public int PublicKeyAlgorithm { get; set; } = -7;

    /// <summary>
    /// Optional friendly name for the credential.
    /// </summary>
    public string? FriendlyName { get; set; }
}
