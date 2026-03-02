using System;

namespace Organization.Shared.Identity;

/// <summary>
/// Represents the request payload for completing a WebAuthn authentication or registration process, containing the unique request identifier, the client's response with the credential information, and an optional friendly name for the credential.
/// This class is used for both login and registration completion, where the server will validate the client's response against the expected challenge and either authenticate the user or register a new credential based on the context of the request.
/// </summary>
public class WebAuthnCompleteRequest
{
    /// <summary>
    /// Gets or sets the unique identifier for the authentication or registration request, used to correlate the client's response with the server's expectations.
    /// </summary>
    [Required(ErrorMessage = "RequestId is required.")]
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the JSON string containing the client's response with the credential information, including the credential ID, public key, signature, and other relevant data required for validating the authentication or registration process.
     /// This JSON string should conform to the WebAuthn specification for the client's response format.
     /// The server will parse this JSON to extract the necessary information for validating the credential and completing the authentication or registration process.
     /// </summary>
    [Required(ErrorMessage = "CredentialJson is required.")]
    public string CredentialJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional user-friendly name for the credential (e.g., "My Security Key"), which can be used to identify the credential in the user's account management interface. This is not required for the authentication or registration process but can enhance the user experience by allowing users to label their credentials.
     /// Maximum length is 100 characters.
    /// </summary>
    [MaxLength(100, ErrorMessage = "FriendlyName cannot exceed 100 characters.")]
    public string? FriendlyName { get; set; }
}
