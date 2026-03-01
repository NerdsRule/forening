
namespace Organization.Shared.Identity;

/// <summary>
/// Result model containing WebAuthn registration data from the client.
/// </summary>
public class WebAuthRegisterClientResult
{
    /// <summary>
    /// The credential identifier returned from the authenticator.
    /// </summary>
    public string CredentialId { get; set; } = string.Empty;

    /// <summary>
    /// The client data JSON containing the challenge and origin information.
    /// </summary>
    public string ClientDataJson { get; set; } = string.Empty;

    /// <summary>
    /// The public key in SPKI format.
    /// </summary>
    public string PublicKeySpki { get; set; } = string.Empty;

    /// <summary>
    /// The public key algorithm identifier (default: -7 for ES256).
    /// </summary>
    public int PublicKeyAlgorithm { get; set; } = -7;
}
