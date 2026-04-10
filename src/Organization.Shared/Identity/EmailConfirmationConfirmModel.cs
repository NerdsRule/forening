namespace Organization.Shared.Identity;

/// <summary>
/// Request payload for confirming a user's email with a token.
/// </summary>
public class EmailConfirmationConfirmModel
{
    [Required(ErrorMessage = "Token is required.")]
    public string? Token { get; set; }
}