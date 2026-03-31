namespace Organization.Shared.Identity;

/// <summary>
/// Request payload for completing a self-service password reset.
/// </summary>
public class SelfResetPasswordModel
{
    [Required(ErrorMessage = "User ID is required.")]
    public string? UserId { get; set; }

    [Required(ErrorMessage = "Reset token is required.")]
    public string? ResetToken { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Confirm Password is required.")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }
}
