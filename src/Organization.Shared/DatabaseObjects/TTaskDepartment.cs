
namespace Organization.Shared.DatabaseObjects;
/// <summary>
/// Represents a link between a task and a department within the organization.
/// Inherits common table metadata and behavior from <see cref="TBaseTable"/>.
/// </summary>
[Table("TTaskDepartments")]
public class TTaskDepartment : TBaseTable
{
/// <summary>
/// Foreign key to the task this department is associated with (required).
/// </summary>
[Required, ForeignKey(nameof(TTask))]
public int TaskId { get; set; }

/// <summary>
/// Navigation property for the linked task.
/// </summary>
public virtual TTask? Task { get; set; }

/// <summary>/ 
/// Foreign key to the department this task is associated with (required).
/// </summary>
[Required, ForeignKey(nameof(TDepartment))]
public int DepartmentId { get; set; }

/// <summary>
/// Navigation property for the linked department.
/// </summary>
public virtual TDepartment? Department { get; set; }
}
