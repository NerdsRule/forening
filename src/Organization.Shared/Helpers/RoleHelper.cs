
namespace Organization.Shared.Helpers;

public static class RoleHelper
{

    /// <summary>
    /// Return roles that are specific to a Organization
    /// </summary>
    /// <returns>Dictionary of RolesEnum</returns>
    public static List<RolesEnum> GetOrganizationRoles()
    {
        return new List<RolesEnum>
        {
            RolesEnum.OrganizationMember,
            RolesEnum.OrganizationAdmin,
            RolesEnum.EnterpriseAdmin,
            RolesEnum.None
        };
    }

    /// <summary>
    /// Return roles that are specific to a Department
    /// </summary>
    /// <returns>Dictionary of RolesEnum</returns>
    public static List<RolesEnum> GetDepartmentRoles()
    {
        return new List<RolesEnum>
        {
            RolesEnum.DepartmentMember,
            RolesEnum.DepartmentAdmin,
            RolesEnum.None
        };
    }
}
