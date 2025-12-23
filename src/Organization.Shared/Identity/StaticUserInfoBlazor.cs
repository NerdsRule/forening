
namespace Organization.Shared.Identity;

/// <summary>
/// Static user info for Blazor clients
/// </summary>
public static class StaticUserInfoBlazor
{
    /// <summary>
    /// User info
    /// </summary>
    public static UserModel? User {get; set;}

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

}
