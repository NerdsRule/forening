namespace Organization.Shared.DatabaseObjects;

[Table("TWebAuthCredentials")]
public class TWebAuthCredential : TBaseTable
{
    /// <summary>
    /// Gets or sets the ID of the application user associated with this WebAuthn credential. This is a foreign key reference to the AspNetUsers table, linking the credential to a specific user account in the identity system.
    /// </summary>
    [Required, ForeignKey("AspNetUsers")]
    public string AppUserId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the associated application user.
    /// </summary>
    public virtual AppUser? AppUser { get; set; }

    /// <summary>
    /// Gets or sets the credential identifier returned from the authenticator. This is a unique string that identifies the credential and is used during authentication to match the client's assertion with the correct credential record in the database.
    /// </summary>
    [Required, MaxLength(512)]
    public string CredentialId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the public key in SPKI format associated with this credential. This is the public key that was generated during registration and is used to verify signatures during authentication. It is stored as a byte array in the database.
    /// </summary>
    [Required]
    public byte[] PublicKeySpki { get; set; } = [];

    /// <summary>
    /// Gets or sets the algorithm identifier for the public key (default: -7 for ES256).
    /// </summary>
    public int PublicKeyAlgorithm { get; set; } = -7;

    /// <summary>
    /// Gets or sets the signature counter for this credential. This counter is incremented by the authenticator each time the credential is used for authentication. It is used to detect cloned authenticators and prevent replay attacks. The server should check that the counter value increases with each authentication and reject any authentication attempts where the counter does not increase.
    /// </summary>
    public uint SignatureCounter { get; set; }

    /// <summary>
    /// Gets or sets a user-friendly name for this credential (max 200 characters).
    /// </summary>
    /// <remarks>
    /// This optional field allows users to label and identify their credentials (e.g., "My USB Key", "iPhone").
    /// </remarks>
    [MaxLength(200)]
    public string? FriendlyName { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this credential was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC timestamp of the last successful authentication using this credential.
    /// </summary>
    /// <remarks>
    /// This is null if the credential has never been used for authentication.
    /// </remarks>
    public DateTime? LastUsedAtUtc { get; set; }
}
