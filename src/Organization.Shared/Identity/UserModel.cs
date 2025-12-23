

namespace Organization.Shared.Identity;

/// <summary>
/// User info returned to the client from the API
/// </summary>
public class UserModel
{
    /// <summary>
    /// User Id
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// User name
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Email of the user
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Points accumulated by the user.
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Used points by the user.
    /// </summary>
    public int UsedPoints { get; set; }

    /// <summary>
    /// Member number associated with the user.
    /// </summary>
    public string? MemberNumber { get; set; }

    /// <summary>
    /// TAppUserOrganizations the user is associated with.
    /// </summary>
    public List<TAppUserOrganization> AppUserOrganizations { get; set; } = [];

    /// <summary>
    /// TAppUserDepartments the user is associated with.
    /// </summary>
    public List<TAppUserDepartment> AppUserDepartments { get; set; } = [];
}
