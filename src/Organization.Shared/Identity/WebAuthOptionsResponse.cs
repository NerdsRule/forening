using System;

namespace Organization.Shared.Identity;

/// <summary>
/// Response model containing WebAuthn registration and authentication options.
/// </summary>
public class WebAuthOptionsResponse
{
    /// <summary>
    /// The challenge string used in the WebAuthn ceremony.
    /// </summary>
    public string Challenge { get; set; } = string.Empty;

    /// <summary>
    /// The Relying Party identifier.
    /// </summary>
    public string RpId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the Relying Party.
    /// </summary>
    public string RpName { get; set; } = "Organization";

    /// <summary>
    /// The user identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The username for the user.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// The display name for the user.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// List of credential IDs allowed for authentication.
    /// </summary>
    public List<string> AllowCredentialIds { get; set; } = [];

    /// <summary>
    /// The timeout in milliseconds for the WebAuthn ceremony.
    /// </summary>
    public int TimeoutMs { get; set; } = 60000;
}