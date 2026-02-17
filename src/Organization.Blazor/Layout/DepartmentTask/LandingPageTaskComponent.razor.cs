
namespace Organization.Blazor.Layout.DepartmentTask;

/// <summary>
/// Task list component for the landing page of the department tasks. This component will display a list of tasks for the selected department, and it will allow users to add new tasks or edit existing tasks if they have the appropriate permissions. The component will also handle the logic for determining which fields and buttons should be enabled or disabled based on the user's permissions and whether they are creating a new task or editing an existing one.
/// </summary>
partial class LandingPageTaskComponent
{
    private TaskListComponent _taskListComponent = null!;
    private FormResultComponent FormResult { get; set; } = null!;
    private bool ShowSpinner { get; set; } = false;
    private List<UserModel> UsersWithAccess => [];
    [Inject] IDepartmentTaskService DepartmentTaskService { get; set; } = null!;

    /// <summary>
    /// Load data from the API after the component is initialized
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Load user info
        await base.OnInitializedAsync();
    }
}
