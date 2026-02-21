
namespace Organization.Core.TaskPrizeCore;

/// <summary>
/// Defines workflows related to tasks within the organization. This class can be expanded in the future to include methods that handle complex task-related operations, such as transitioning task statuses, assigning tasks based on specific criteria, or implementing business rules related to task management.
/// </summary>
public static class TaskWorkflows
{
    public static Dictionary<Shared.TaskStatusEnum, string> StatusTextMapping = new()
    {
        { Shared.TaskStatusEnum.NotStarted, "Not Started" },
        { Shared.TaskStatusEnum.InProgress, "In Progress" },
        { Shared.TaskStatusEnum.Completed, "Completed" },
        { Shared.TaskStatusEnum.VerifiedCompleted, "Verified" }
    };

        /// <summary>
        /// Get the next task status based on the current status and the user's role. This method can be used to determine what the next logical status of a task should be when a user takes an action on it, such as marking it as in progress or completed. The available transitions can vary based on the user's role, ensuring that only users with the appropriate permissions can move tasks to certain statuses.
        /// </summary>
        /// <param name="currentStatus">The current status of the task.</param>
        /// <param name="userRole">The roles of the user performing the action.</param>
        /// <param name="isAssignedToMe">Indicates whether the task is assigned to the current user.</param>
        /// <returns>A tuple containing the text representation of the next status and its corresponding enum value.</returns>
    public static (string text, Shared.TaskStatusEnum enumValue)[] GetAvailableTaskStatus(Shared.TaskStatusEnum currentStatus, Shared.RolesEnum[] userRole, bool isAssignedToMe)
    {
        var availableStatuses = new List<(string text, Shared.TaskStatusEnum enumValue)>();
        if (userRole.Contains(Shared.RolesEnum.DepartmentAdmin) || userRole.Contains(Shared.RolesEnum.OrganizationAdmin) || userRole.Contains(Shared.RolesEnum.EnterpriseAdmin))
        {
            availableStatuses.AddRange(
            [
                (StatusTextMapping[Shared.TaskStatusEnum.NotStarted], Shared.TaskStatusEnum.NotStarted),
                (StatusTextMapping[Shared.TaskStatusEnum.InProgress], Shared.TaskStatusEnum.InProgress),
                (StatusTextMapping[Shared.TaskStatusEnum.Completed], Shared.TaskStatusEnum.Completed),
                (StatusTextMapping[Shared.TaskStatusEnum.VerifiedCompleted], Shared.TaskStatusEnum.VerifiedCompleted)
            ]);
        }
        else if (userRole.Contains(Shared.RolesEnum.DepartmentMember) && isAssignedToMe)
        {
            availableStatuses.AddRange(
            [
                (StatusTextMapping[Shared.TaskStatusEnum.NotStarted], Shared.TaskStatusEnum.NotStarted),
                (StatusTextMapping[Shared.TaskStatusEnum.InProgress], Shared.TaskStatusEnum.InProgress),
                (StatusTextMapping[Shared.TaskStatusEnum.Completed], Shared.TaskStatusEnum.Completed)
            ]);
        }
        else
        {
            availableStatuses.Add((StatusTextMapping[currentStatus], currentStatus));
        }
        if (!availableStatuses.Any(s => s.enumValue == currentStatus))
        {            
            availableStatuses.Add((StatusTextMapping[currentStatus], currentStatus));
        }
        return [.. availableStatuses];
    }
}
