
namespace Organization.Shared.Identity;

/// <summary>
/// Form for organization and department selection
/// </summary>
public class OrganizationDepartmentForm
{
    /// <summary>
    /// List of organizations with departments
    /// </summary>
    public List<TOrganization> Organizations { get; set; } = [];

    /// <summary>
    /// List of departments for the selected organization
    /// </summary>
    public List<TDepartment> Departments { get; set; } = [];

    /// <summary>
    /// Selected organization id
    /// </summary>
    [Required(ErrorMessage = "Organization is required")]
    public int? SelectedOrganizationId { get; set; }

    /// <summary>
    /// Selected department id
    /// </summary>
    [Required(ErrorMessage = "Department is required")]
    public int? SelectedDepartmentId { get; set; }

    /// <summary>
    /// Selected organization
    /// </summary>
    public TOrganization? SelectedOrganization { get; set; }

    /// <summary>
    /// Selected department
    /// </summary>
    public TDepartment? SelectedDepartment { get; set; }
}
