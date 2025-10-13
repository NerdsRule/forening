
namespace Organization.Shared.DatabaseObjects;
/// <summary>
/// Represents a link between an application user and a department within the organization.
/// Inherits common table metadata and behavior from <see cref="TBaseTable"/>.
/// </summary>
/// <remarks>
/// This class is intended to act as a data transfer / entity object used by
/// the data access layer or an ORM. Extend this class with properties that
/// map to the columns of the underlying "AppUserDepartment" table (for example,
/// AppUserId, DepartmentId, Role, etc.).
/// Keep this class focused on data representation; business logic should be
/// placed in services or domain classes.
/// </remarks>
/// <seealso cref="TBaseTable"/>
[Table("TAppUserDepartments")]
public class TAppUserDepartment : TBaseTable
{
    /// <summary>
    /// Foreign key to the user this department belongs to (required).
    /// </summary>
    [Required, ForeignKey("AspNetUsers")]
    public string AppUserId { get; set; } = null!;

    /// <summary>
    /// Navigation property for the linked user.
    /// </summary>
    public virtual AppUser? AppUser { get; set; }

    /// <summary>
    /// Foreign key to the department this user belongs to (required).
    /// </summary>
    [Required, ForeignKey(nameof(TDepartment))]
    public int DepartmentId { get; set; }

    /// <summary>
    /// Navigation property for the linked department.
    /// </summary>
    public virtual TDepartment? Department { get; set; }

    /// <summary>
    /// Role of the user within the department.
    /// </summary>
    public DepartmentRolesEnum Role { get; set; } = DepartmentRolesEnum.Member;
}
