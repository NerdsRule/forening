
namespace Organization.Shared.Interfaces;

/// <summary>
/// Interface for managing department tasks and tasks within departments
/// </summary>
public interface IDepartmentTaskService
{
    #region Task Management
    /// <summary>
    /// Get all tasks for a specific department
    /// </summary>
    /// <param name="departmentId">The ID of the department</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A list of tasks associated with the department</returns>
    Task<(List<TTask>? data, FormResult? formResult)> GetTasksByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Get tasks that are owned by a specific department
    /// </summary>
    /// <param name="departmentId">The ID of the department</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A list of tasks owned by the department</returns>
    Task<(List<TTask>? data, FormResult? formResult)> GetOwnedTasksByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Add or update a task
    /// </summary>
    /// <param name="task">The task to add or update</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The added or updated task, or an error if the operation failed</returns>
    Task<(TTask? data, FormResult? formResult)> AddUpdateTaskAsync(TTask task, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a task </summary>
    /// <param name="taskId">The ID of the task to delete</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A result indicating whether the deletion was successful or if there was an error</returns>
    Task<FormResult> DeleteTaskAsync(int taskId, CancellationToken cancellationToken);
    #endregion

    #region Department Task Management
    /// <summary>
    /// Add a TTaskDepartment association
    /// </summary>
    /// <param name="departmentTask">The TTaskDepartment association to add</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The added TTaskDepartment association, or an error if the operation failed</returns>
    Task<(TTaskDepartment? data, FormResult? formResult)> AddDepartmentTaskAsync(TTaskDepartment departmentTask, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a TTaskDepartment association
    /// </summary>
    /// <param name="departmentTaskId">The ID of the TTaskDepartment association to delete</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A result indicating whether the deletion was successful or if there was an error</returns>
    Task<FormResult> DeleteDepartmentTaskAsync(int departmentTaskId, CancellationToken cancellationToken);
    #endregion

    #region Users with access to a department and Organization
    /// <summary>
    /// Get users with access to a specific department
    /// </summary>
    /// <param name="departmentId">The ID of the department</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A list of users who have access to the department</returns>
    Task<(List<UserModel>? data, FormResult? formResult)> GetUsersWithAccessToDepartmentAsync(int departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Get users with access to a specific organization
    /// </summary>
    /// <param name="organizationId">The ID of the organization</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A list of users who have access to the organization</returns>
    Task<(List<UserModel>? data, FormResult? formResult)> GetUsersWithAccessToOrganizationAsync(int organizationId, CancellationToken cancellationToken);
    #endregion

    #region Task Points Awarded
    /// <summary>
    /// Get a list of tasks that have points awarded for a specific department
    /// </summary>
    /// <param name="departmentId">The ID of the department</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A list of tasks with points awarded for the department</returns>
    Task<(List<VTaskPointsAwarded>? data, FormResult? formResult)> GetTasksWithPointsAwardedByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken);

     /// <summary>
    /// Get VTaskPointsAwarded for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A list of tasks with points awarded for the user</returns>
    public Task<(List<VTaskPointsAwarded>? data, FormResult? formResult)> GetTasksWithPointsAwardedByUserIdAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Get top users with points awarded for a specific department, including the specified user if they are not in the top users.
    /// </summary> <param name="userId">The ID of the user to include if not in top users</param>
    /// <param name="departmentId">The ID of the department</param>
    /// <param name="topCount">The number of top users to retrieve</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A list of top users with points awarded for the department, including the specified user if they are not in the top users</returns>
    public Task<(List<VTaskPointsAwarded>? data, FormResult? formResult)> GetTopUsersWithPointsAwardedByDepartmentAsync(string userId, int departmentId, int topCount, CancellationToken cancellationToken);
    #endregion
}
