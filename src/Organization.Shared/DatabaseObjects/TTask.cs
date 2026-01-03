
namespace Organization.Shared.DatabaseObjects;

/// <summary>
/// Represents a task for a department.
/// </summary>
[Table("TTasks")]
public class TTask : TBaseTable
{
    /// <summary>
    /// Gets or sets the name of the task.
    /// </summary>
    [Required(ErrorMessage = "Name of task is required."), MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the task.
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the estimated time to complete the task, in minutes.
    /// </summary>
    [Required(ErrorMessage = "Estimated time is required.")]
    public double EstimatedTimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the task is due.
    /// </summary>
    [Required(ErrorMessage = "Due date is required.")]
    public DateTime DueDateUtc { get; set; }

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
}
