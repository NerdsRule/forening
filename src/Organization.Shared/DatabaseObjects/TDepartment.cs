

/// <summary>
/// Represents a department entity within the application's shared database objects.
/// </summary>
/// <remarks>
/// Inherits common table metadata and behavior from <see cref="TBaseTable"/> (for example: primary key, audit fields,
/// and other shared properties). This class is intended to be extended with department-specific fields
/// such as Name, Code, Description, or relationships to other organizational entities.
/// Use it as the domain model mapped to the corresponding Departments table in the persistence layer.
/// </remarks>
/// <seealso cref="TBaseTable"/>
namespace Organization.Shared.DatabaseObjects;
[Table("TDepartments")]
public class TDepartment : TBaseTable
{
    /// <summary>
    /// Department name (required).
    /// </summary>
    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Optional short code for the department (e.g. "HR").
    /// </summary>
    [MaxLength(20)]
    public string? Code { get; set; }

    /// <summary>
    /// Optional longer description of the department.
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the department is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Foreign key to the organization this department belongs to (required).
    /// </summary>
    [Required, ForeignKey(nameof(OrganizationId))]
    public int OrganizationId { get; set; }

    /// <summary>
    /// Navigation property for the linked organization.
    /// </summary>
    public virtual TOrganization? Organization { get; set; }

}
