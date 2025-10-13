
/// <summary>
/// Represents the data required to register a new user account.
/// Contains credentials, basic profile information and optional organization association
/// used by registration endpoints and validation logic.
/// </summary>
/// <remarks>
/// Validate Email, Password and ConfirmPassword on the server side before creating a user.
/// Do not persist Password or ConfirmPassword in plain text; handle securely according to best practices.
/// AcceptTerms should be true for the registration to be accepted.
/// </remarks>

/// <summary>
/// Gets or sets the username for the new account.
/// </summary>

/// <summary>
/// Gets or sets the email address for the new account.
/// Expected to be a valid email format and unique within the system.
/// </summary>

/// <summary>
/// Gets or sets the password for the new account.
/// Must meet configured password complexity rules. Treat this value as sensitive.
/// </summary>

/// <summary>
/// Gets or sets the confirmation of the Password.
/// Used to verify that the user entered the intended password correctly.
/// </summary>

/// <summary>
/// Gets or sets the user's given name (first name).
/// </summary>

/// <summary>
/// Gets or sets the user's family name (last name).
/// </summary>

/// <summary>
/// Gets or sets the user's phone number.
/// Optional; validate format according to application requirements.
/// </summary>

/// <summary>
/// Gets or sets the optional identifier of the organization the user belongs to.
/// Null or empty when the user is not associated with any organization.
/// </summary>

/// <summary>
/// Gets or sets a value indicating whether the user has accepted the terms and conditions.
/// Registration should typically require this to be true.
/// </summary>
namespace Organization.Shared.Identity;

public class RegisterModel
{
    /// <summary>
    /// Gets or sets the username for the new account.
    /// </summary>
    public string? UserName { get; set; }
    /// <summary>
    /// Gets or sets the email address for the new account.
    /// Expected to be a valid email format and unique within the system.
    /// </summary>
    public string? Email { get; set; }
    /// <summary>
    /// Gets or sets the password for the new account.
    /// Must meet configured password complexity rules. Treat this value as sensitive.
    /// </summary>
    public required string Password { get; set; }
    /// <summary>
    /// Gets or sets the confirmation of the Password.
    /// Used to verify that the user entered the intended password correctly.
    /// </summary>
    public required string ConfirmPassword { get; set; }
    /// <summary>
    /// Gets or sets the user's given name (first name).
    /// </summary>
    public string? FirstName { get; set; }
    /// <summary>
    /// Gets or sets the user's family name (last name).
    /// </summary>
    public string? LastName { get; set; }
    /// <summary>
    /// Gets or sets the user's phone number.
    /// Optional; validate format according to application requirements.
    /// </summary>
    public string? PhoneNumber { get; set; }
    /// <summary>
    /// Gets or sets the optional identifier of the organization the user belongs to.
    /// Null or empty when the user is not associated with any organization.
    /// </summary>
    public int OrganizationId { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the user has accepted the terms and conditions.
    /// Registration should typically require this to be true.
    /// </summary>
    public bool AcceptTerms { get; set; }
}
