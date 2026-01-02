
namespace Organization.Shared.Identity;

public class ResetPasswordModel
{
    /// <summary>
    /// Gets or sets the email address associated with the account.
    /// Expected to be a valid email format.
    /// </summary>
    [Required(ErrorMessage = "User ID is required.")]
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the new password for the account.
    /// Must meet configured password complexity rules. Treat this value as sensitive.
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the confirmation of the new Password.
    /// Used to verify that the user entered the intended password correctly.
    /// </summary>
    [Required(ErrorMessage = "Confirm Password is required.")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }
    
}
