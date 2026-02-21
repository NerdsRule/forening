
using Organization.Shared.DatabaseObjects;


/// <summary>
/// Represents an application user in the identity system.
/// Inherits from <see cref="IdentityUser"/> to provide user authentication and authorization features.
/// </summary>
namespace Organization.Shared.Identity;

public class AppUser : IdentityUser
{
    /// <summary>
    /// Member number associated with the user.
    /// </summary>
    public string? MemberNumber { get; set; }

    /// <summary>
    /// Display name of the user, which can be used in the UI instead of the username or email.
    /// </summary>
    [MaxLength(200, ErrorMessage = "Display name cannot exceed 200 characters.")]
    public string? DisplayName { get; set; }
}
