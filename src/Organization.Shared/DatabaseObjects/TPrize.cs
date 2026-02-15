
namespace Organization.Shared.DatabaseObjects;

/// <summary>
/// Represents a prize entry that a user can select.
/// </summary>
[Table("TPrizes")]
public class TPrize : TBaseTable
{
    /// <summary>
    /// Gets or sets the name of the prize entry.
    /// </summary>
    [Required(ErrorMessage = "Name of prize is required."), MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the prize entry.
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Points this prize entry costs.
    /// </summary>
    [Required(ErrorMessage = "Points cost is required.")]
    public int PointsCost { get; set; }

    /// <summary>
    /// Gets or sets the department ID associated with the task.
    /// </summary>
    [Required(ErrorMessage = "Department ID is required."), ForeignKey(nameof(TDepartment))]
    public int DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the user ID that created the task.
    /// </summary>
    [Required(ErrorMessage = "Creator user ID is required."), ForeignKey("AspNetUsers")]
    public string CreatorUserId { get; set; } = null!;

    /// <summary>
    /// Refers to the user.
    /// </summary>
    public virtual AppUser? CreatorUser { get; set; }

    /// <summary>
    /// Gets or sets the user ID assigned to the task.
    /// </summary>
    [ForeignKey("AspNetUsers")]
    public string? AssignedUserId { get; set; }

    /// <summary>
    /// Refers to the assigned user.
    /// </summary>
    public virtual AppUser? AssignedUser { get; set; }

    /// <summary>
    /// Status of the prize entry.
    /// </summary>
    [Required(ErrorMessage = "Status is required.")]
    public PrizeStatusEnum Status { get; set; } = PrizeStatusEnum.Available;
}
