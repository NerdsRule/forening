
namespace Organization.Core.TaskPrizeCore;

/// <summary>
/// Defines workflows related to prizes within the organization. This class can be expanded in the future to include methods that handle complex prize-related operations, such as transitioning prize statuses, assigning prizes based on specific criteria, or implementing business rules related to prize management.
/// </summary>
public static class PrizeWorkflows
{
    public static Dictionary<Shared.PrizeStatusEnum, string> StatusTextMapping = new()
    {
        { Shared.PrizeStatusEnum.Available, "Available" },
        { Shared.PrizeStatusEnum.PendingRedemption, "Pending Redemption" },
        { Shared.PrizeStatusEnum.Redeemed, "Redeemed" }
    };

    /// <summary>
    /// Returns the statuses a user can select based on role and assignment.
    /// </summary>
    /// <param name="currentStatus">Current prize status.</param>
    /// <param name="userRole">Roles of the current user.</param>
    /// <param name="isAssignedToMe">True when the prize is assigned to the current user.</param>
    /// <returns>Tuple array with display text and enum value.</returns>
    public static (string text, Shared.PrizeStatusEnum enumValue)[] GetAvailablePrizeStatus(Shared.PrizeStatusEnum currentStatus, Shared.RolesEnum[] userRole, bool isAssignedToMe)
    {
        var availableStatuses = new List<(string text, Shared.PrizeStatusEnum enumValue)>();

        if (userRole.Contains(Shared.RolesEnum.DepartmentAdmin) || userRole.Contains(Shared.RolesEnum.OrganizationAdmin) || userRole.Contains(Shared.RolesEnum.EnterpriseAdmin))
        {
            availableStatuses.AddRange(
            [
                (StatusTextMapping[Shared.PrizeStatusEnum.Available], Shared.PrizeStatusEnum.Available),
                (StatusTextMapping[Shared.PrizeStatusEnum.PendingRedemption], Shared.PrizeStatusEnum.PendingRedemption),
                (StatusTextMapping[Shared.PrizeStatusEnum.Redeemed], Shared.PrizeStatusEnum.Redeemed)
            ]);
        }
        else if (userRole.Contains(Shared.RolesEnum.DepartmentMember) && isAssignedToMe)
        {
            availableStatuses.AddRange(
            [
                (StatusTextMapping[Shared.PrizeStatusEnum.Available], Shared.PrizeStatusEnum.Available),
                (StatusTextMapping[Shared.PrizeStatusEnum.PendingRedemption], Shared.PrizeStatusEnum.PendingRedemption)
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

    /// <summary>
    /// Determines the next status of a prize based on its current status and the user's roles.
    /// </summary> <param name="currentStatus">The current status of the prize.</param>
    /// <param name="userRole">The roles of the user attempting to change the prize status.</param>
    /// <returns>A tuple containing the text for the next action and the next status of the prize.</returns>
    public static (string text, Shared.PrizeStatusEnum nextStatus) GetNextPrizeStatus(Shared.PrizeStatusEnum currentStatus, Shared.RolesEnum[] userRole)
    {
        if (userRole.Contains(Shared.RolesEnum.DepartmentAdmin) || userRole.Contains(Shared.RolesEnum.OrganizationAdmin) || userRole.Contains(Shared.RolesEnum.EnterpriseAdmin))
        {
            return currentStatus switch
            {
                Shared.PrizeStatusEnum.Available => ("Request Prize", Shared.PrizeStatusEnum.PendingRedemption),
                Shared.PrizeStatusEnum.PendingRedemption => ("Redeem Prize", Shared.PrizeStatusEnum.Redeemed),
                Shared.PrizeStatusEnum.Redeemed => ("Make Prize Available", Shared.PrizeStatusEnum.Available),
                _ => ("Unknown status", currentStatus)
            };
        }
        else
        {
            return ("You do not have permission to change the prize status", currentStatus);
        }
    }
}
