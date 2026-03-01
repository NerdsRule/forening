
namespace Organization.Shared.Identity;

/// <summary>
/// Represents the result of initiating a WebAuthn passkey registration process, containing the unique request identifier and the options required for the client to create a new credential.
/// </summary>
public class WebAuthnBeginPasskeyRegistrationResult
{
        /// <summary>
        /// Gets or sets the unique identifier for the registration request, used to correlate the client's response with the server's expectations.
        /// </summary>
    public string RequestId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the JSON element containing the options required for the client to create a new credential.
    /// </summary>
    public JsonElement Options { get; set; }
}
