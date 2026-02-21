

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
    [Required(ErrorMessage = "User name is required")]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Display name of the user, which can be used in the UI instead of the username or email.
    /// </summary>
    [MaxLength(200, ErrorMessage = "Display name cannot exceed 200 characters.")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Email of the user
    /// </summary>
    public string Email { get; set; } = null!;

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
