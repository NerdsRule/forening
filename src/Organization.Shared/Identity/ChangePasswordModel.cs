

/// <summary>
/// Represents the input model used to change a user's password.
/// </summary>
/// <remarks>
/// Both properties are required. <see cref="CurrentPassword"/> is used to verify the user's existing credentials,
/// and <see cref="NewPassword"/> is the replacement password to be set for the account. Ensure <c>NewPassword</c>
/// complies with the application's password complexity, length, and security policies (e.g. minimum length, character types).
/// </remarks>

/// <summary>
/// The user's current password used to validate identity before allowing a password change.
/// </summary>
/// <value>Required. Plain-text password provided by the user for verification.</value>

/// <summary>
/// The new password to assign to the user's account.
/// </summary>
/// <value>Required. Must satisfy configured password complexity and security requirements.</value>
namespace Organization.Shared.Identity;

public class ChangePasswordModel
{
    /// <summary>
    /// The user's current password used to validate identity before allowing a password change.
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{8,}$", 
        ErrorMessage = "Password must be at least 8 characters long and contain at least one lowercase letter, one uppercase letter, one number, and one special character.")]
    public required string CurrentPassword { get; set; }

    /// <summary>
    /// The new password to assign to the user's account.
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{8,}$", 
        ErrorMessage = "Password must be at least 8 characters long and contain at least one lowercase letter, one uppercase letter, one number, and one special character.")]
    public required string NewPassword { get; set; }

}
