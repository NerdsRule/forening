
/// <summary>
/// Represents an organization record in the database.
/// Inherits common table metadata and behavior from <see cref="TBaseTable"/>.
/// </summary>
/// <remarks>
/// This class is intended to act as a data transfer / entity object used by
/// the data access layer or an ORM. Extend this class with properties that
/// map to the columns of the underlying "Organization" table (for example,
/// Name, Address, CreatedAt, IsActive, etc.).
///
/// Keep this class focused on data representation; business logic should be
/// placed in services or domain classes.
/// </remarks>
/// <seealso cref="TBaseTable"/>
namespace Organization.Shared.DatabaseObjects;
[Table("TOrganizations")]
public class TOrganization : TBaseTable
{
    /// <summary>
    /// The name of the organization.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// The address of the organization.
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// The contact email for the organization.
    /// </summary>
    [EmailAddress]
    [MaxLength(100)]
    public string? ContactEmail { get; set; }

    /// <summary>
    /// The contact phone number for the organization.
    /// </summary>
    [Phone]
    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Indicates whether the organization is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The date and time when the organization record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The date and time when the organization record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
}
