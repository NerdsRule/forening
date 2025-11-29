
namespace Organization.Shared.DatabaseObjects;
/// <summary>
/// Represents a link between an application user and an organization.
/// Inherits common table metadata and behavior from <see cref="TBaseTable"/>.
/// </summary>
/// <remarks>
/// This class is intended to act as a data transfer / entity object used by
/// the data access layer or an ORM. Extend this class with properties that
/// map to the columns of the underlying "AppUserOrganization" table (for example,
/// AppUserId, OrganizationId, Role, etc.).
/// Keep this class focused on data representation; business logic should be
/// placed in services or domain classes.
/// </remarks>
/// <seealso cref="TBaseTable"/>
[Table("TAppUserOrganizations")]
public class TAppUserOrganization : TBaseTable
{
    /// <summary>
    /// Foreign key to the user this organization belongs to (required).
    /// </summary>
    [Required, ForeignKey("AspNetUsers")]
    public string AppUserId { get; set; } = null!;

    /// <summary>
    /// Navigation property for the linked user.
    /// </summary>
    public virtual AppUser? AppUser { get; set; }

    /// <summary>
    /// Foreign key to the organization this user belongs to (required).
    /// </summary>
    [Required, ForeignKey(nameof(TOrganization))]
    public int OrganizationId { get; set; }

    /// <summary>
    /// Navigation property for the linked organization.
    /// </summary>
    public virtual TOrganization? Organization { get; set; }

    /// <summary>
    /// Role of the user within the organization.
    /// </summary>
    public RolesEnum Role { get; set; } = RolesEnum.OrganizationMember;
}
