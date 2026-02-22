namespace Organization.Shared.DatabaseObjects;

/// <summary>
/// Represents a view of tasks that have points awarded with user and department information.
/// </summary>
public class VTaskPointsAwarded
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user display name, which can be used in the UI instead of the username or email.
    /// </summary>
    public string? UserDisplayName { get; set; }

    /// <summary>
    /// Gets or sets the user email.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task ID.
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// Gets or sets the task name.
    /// </summary>
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task description.
    /// </summary>
    public string? TaskDescription { get; set; }

    /// <summary>
    /// Gets or sets the task status.
    /// </summary>
    public Shared.TaskStatusEnum TaskStatus { get; set; }

    /// <summary>
    /// Gets or sets the points awarded for the task.
    /// </summary>
    public int TaskPointsAwarded { get; set; }

    /// <summary>
    /// Gets or sets the department ID.
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the department name.
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>
    /// User position in the department, used for sorting users within a department.
    /// </summary> <remarks>
    /// This is not a database field, but is calculated in the application based on the user's position within the department.
    /// </remarks>
    public int UserRanking { get; set; }
}