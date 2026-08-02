using System.Security.Claims;

namespace Organization.Shared.Interfaces;

/// <summary>
/// Interface for the UI state service, which manages UI state and notifies components of user updates.
/// </summary>
public interface IUiStateService
{
    /// <summary>
    /// Event that is triggered when the user information is updated. Components can subscribe to this event to refresh their data when the user is updated.
    /// </summary>
    event Action? UserUpdatedEvent;

    /// <summary>
    /// Method to invoke the UserUpdatedEvent, notifying all subscribers that the user information has been updated.
    /// </summary>
    void NotifyUserUpdated();

    /// <summary>
    /// Current authenticated principal for the active user session.
    /// </summary>
    ClaimsPrincipal? CurrentPrincipal { get; }

    /// <summary>
    /// Current authenticated user for the active session.
    /// </summary>
    UserModel? User { get; }

    /// <summary>
    /// Current selected organization in the active UI session.
    /// </summary>
    TAppUserOrganization? SelectedOrganization { get; }

    /// <summary>
    /// Current selected department in the active UI session.
    /// </summary>
    TAppUserDepartment? SelectedDepartment { get; }

    /// <summary>
    /// Sets the current authenticated principal.
    /// </summary>
    void SetCurrentPrincipal(ClaimsPrincipal? principal);

    /// <summary>
    /// Sets the current user.
    /// </summary>
    void SetUser(UserModel? user);

    /// <summary>
    /// Sets the selected organization.
    /// </summary>
    void SetSelectedOrganization(TAppUserOrganization? selectedOrganization);

    /// <summary>
    /// Sets the selected department.
    /// </summary>
    void SetSelectedDepartment(TAppUserDepartment? selectedDepartment);

    /// <summary>
    /// Gets the role claims from the current authenticated principal.
    /// </summary>
    RolesEnum[] CurrentRoles { get; }

    /// <summary>
    /// Checks whether the current principal has the specified role.
    /// </summary>
    bool HasRole(RolesEnum role);

    /// <summary>
    /// Checks whether the current principal has any of the specified roles.
    /// </summary>
    bool HasAnyRole(params RolesEnum[] roles);
}
