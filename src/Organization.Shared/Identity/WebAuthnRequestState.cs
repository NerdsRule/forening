
namespace Organization.Shared.Identity;

/// <summary>
///    Represents the state of an ongoing WebAuthn request, including the user ID and the options JSON that will be sent to the client for performing WebAuthn operations.
///    This class is used to temporarily store the necessary information for a WebAuthn registration or authentication process, allowing the server to correlate the request with the correct user and provide the appropriate options for the client to use when interacting with the WebAuthn APIs.
/// </summary>
public sealed class WebAuthnRequestState
{
    /// <summary>
    /// The ID of the user associated with this WebAuthn request, used for correlating the request with the correct user during the authentication or registration process.
    /// </summary>
    public string UserId { get; init; } = string.Empty;
    /// <summary>
    /// The JSON string containing the options for the WebAuthn operation (either registration or login), which will be sent to the client to perform the necessary WebAuthn operations using JavaScript APIs.
    /// </summary>
    public string OptionsJson { get; init; } = string.Empty;
}