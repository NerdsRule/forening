/// <summary>
/// Represents a WebAuthn challenge stored in the database for user authentication.
/// </summary>
/// <remarks>
/// This entity is used to manage WebAuthn authentication challenges during the registration
/// and authentication flow. Each challenge is associated with a specific user and has an expiration time.
/// </remarks>
[Table("TWebAuthChallenges")]
public class TWebAuthChallenge : TBaseTable
{
    /// <summary>
    /// Gets or sets the ID of the associated application user.
    /// </summary>
    /// <remarks>
    /// This is a required foreign key that references the AspNetUsers table.
    /// </remarks>
    [Required, ForeignKey("AspNetUsers")]
    public string AppUserId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the associated application user.
    /// </summary>
    public virtual AppUser? AppUser { get; set; }

    /// <summary>
    /// Gets or sets the purpose of this WebAuthn challenge (e.g., "registration", "authentication").
    /// </summary>
    /// <remarks>
    /// Maximum length is 200 characters. This field is required.
    /// </remarks>
    [Required, MaxLength(200)]
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cryptographic challenge string used in the WebAuthn ceremony.
    /// </summary>
    /// <remarks>
    /// Maximum length is 256 characters. This field is required.
    /// </remarks>
    [Required, MaxLength(256)]
    public string Challenge { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the origin URL where the authentication challenge was initiated.
    /// </summary>
    /// <remarks>
    /// Maximum length is 256 characters. This field is required.
    /// </remarks>
    [Required, MaxLength(256)]
    public string Origin { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relying party identifier for this WebAuthn challenge.
    /// </summary>
    /// <remarks>
    /// Maximum length is 200 characters. This field is required.
    /// </remarks>
    [Required, MaxLength(200)]
    public string RelyingPartyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp when this challenge was created.
    /// </summary>
    /// <remarks>
    /// Defaults to the current UTC time when a new instance is created.
    /// </remarks>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC timestamp when this challenge expires.
    /// </summary>
    /// <remarks>
    /// After this time, the challenge should no longer be accepted for authentication.
    /// </remarks>
    public DateTime ExpiresAtUtc { get; set; }
}
