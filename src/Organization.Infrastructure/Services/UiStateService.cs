using System.Security.Claims;

namespace Organization.Infrastructure.Services;

/// <summary>
/// Service to manage UI state, such as notifying components of user updates.
/// </summary>
public class UiStateService : IUiStateService
{
    private ClaimsPrincipal? _currentPrincipal;
    private UserModel? _user;
    private TAppUserOrganization? _selectedOrganization;
    private TAppUserDepartment? _selectedDepartment;

    /// <summary>
    /// Event that is triggered when the user information is updated. Components can subscribe to this event to refresh their data when the user is updated.
    /// </summary>
    public event Action? UserUpdatedEvent;

    /// <inheritdoc />
    public ClaimsPrincipal? CurrentPrincipal => _currentPrincipal;

    /// <inheritdoc />
    public UserModel? User => _user;

    /// <inheritdoc />
    public TAppUserOrganization? SelectedOrganization => _selectedOrganization;

    /// <inheritdoc />
    public TAppUserDepartment? SelectedDepartment => _selectedDepartment;

    /// <inheritdoc />
    public RolesEnum[] CurrentRoles => _currentPrincipal?.Claims
        .Where(claim => claim.Type == ClaimTypes.Role)
        .Select(claim => Enum.TryParse<RolesEnum>(claim.Value, out var role) ? role : RolesEnum.None)
        .Where(role => role != RolesEnum.None)
        .Distinct()
        .ToArray() ?? [];

    /// <summary>
    /// Method to invoke the UserUpdatedEvent, notifying all subscribers that the user information has been updated.
    /// </summary>
    public void NotifyUserUpdated()
    {
        UserUpdatedEvent?.Invoke();
    }

    /// <inheritdoc />
    public void SetCurrentPrincipal(ClaimsPrincipal? principal)
    {
        _currentPrincipal = principal;
    }

    /// <inheritdoc />
    public void SetUser(UserModel? user)
    {
        _user = user;
    }

    /// <inheritdoc />
    public void SetSelectedOrganization(TAppUserOrganization? selectedOrganization)
    {
        _selectedOrganization = selectedOrganization;
    }

    /// <inheritdoc />
    public void SetSelectedDepartment(TAppUserDepartment? selectedDepartment)
    {
        _selectedDepartment = selectedDepartment;
    }

    /// <inheritdoc />
    public bool HasRole(RolesEnum role) => CurrentRoles.Contains(role);

    /// <inheritdoc />
    public bool HasAnyRole(params RolesEnum[] roles)
    {
        if (roles.Length == 0)
        {
            return false;
        }

        foreach (var role in roles)
        {
            if (HasRole(role))
            {
                return true;
            }
        }

        return false;
    }
}
