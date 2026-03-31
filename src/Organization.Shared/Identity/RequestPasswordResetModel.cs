namespace Organization.Shared.Identity;

/// <summary>
/// Request payload for initiating a password reset by email.
/// </summary>
public class RequestPasswordResetModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    public string? Email { get; set; }
}
