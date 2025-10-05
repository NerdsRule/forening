
using Organization.Shared.DatabaseObjects;


/// <summary>
/// Represents an application user in the identity system.
/// Inherits from <see cref="IdentityUser"/> to provide user authentication and authorization features.
/// </summary>
namespace Organization.Shared.Identity;

public class AppUser : IdentityUser
{
    /// <summary>
    /// Points accumulated by the user.
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Used points by the user.
    /// </summary>
    public int UsedPoints { get; set; }

    /// <summary>
    /// Organization membership identifier.
    /// </summary>
    public string? OrganizationMembershipId { get; set; }
}
