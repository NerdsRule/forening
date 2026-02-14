
using System.Diagnostics;

namespace Organization.Shared.Identity;

/// <summary>
/// Static user info for Blazor clients
/// </summary>
public static class StaticUserInfoBlazor
{
    /// <summary>
    /// User info
    /// </summary>
    private static UserModel? _user;
    public static UserModel? User
    {
        get => _user;
        set
        {
            _user = value;
            Console.WriteLine($"User changed: {value?.Id ?? "null"}");
        }
    }

    /// <summary>
    /// Selected TAppUserOrganization
    /// </summary>
    public static TAppUserOrganization? SelectedOrganization { get; set; }

    /// <summary>
    /// Organization role of the selected organization
    /// </summary>
    public static RolesEnum OrganizationRole => SelectedOrganization != null ? SelectedOrganization.Role : RolesEnum.None;
    
    /// <summary>
    /// Selected TAppUserDepartment
    /// </summary>
    public static TAppUserDepartment? SelectedDepartment { get; set; }

    /// <summary>
    /// Department role of the selected department
    /// </summary>
    public static RolesEnum DepartmentRole => SelectedDepartment != null ? SelectedDepartment.Role : RolesEnum.None;

    /// <summary>
    /// Key used for local storage and user settings
    /// </summary>
    public const string UserLocalStorageKey = "UserInfo";

}
