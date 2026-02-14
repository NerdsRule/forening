
using System.Runtime.InteropServices;

namespace Organization.Blazor.Pages;

public partial class TaskEdit
{
    private TaskListComponent _taskListComponent = null!;
    private FormResult? FormResult { get; set; } = null;
    private TTask _newTask { get; set; } = new TTask();
    private bool ShowSpinner { get; set; } = true;
    private List<UserModel> UsersWithAccessToOrganization { get; set; } = [];
    private List<UserModel> UsersWithAccessToDepartment { get; set; } = [];
    private List<UserModel> UsersWithAccess => [.. UsersWithAccessToOrganization.Union(UsersWithAccessToDepartment)];
    [Inject] NavigationManager Navigation { get; set; } = null!;
    [Inject] IDepartmentTaskService DepartmentTaskService { get; set; } = null!;
    /// <summary>
    /// Handle the event when a task is added or updated in the DepartmentTaskComponent. This method will be called with the task that was added or updated, and it can be used to perform any necessary actions, such as displaying a success message or refreshing a list of tasks.
    /// </summary>
    /// <param name="task">The task that was added or updated.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task OnTaskAddedOrUpdated(TTask task)
    {
        BuildEmptyTask();
        _taskListComponent.AddTaskToList(task);
    }

    /// <summary>
    /// Build empty task object with default values for the form in DepartmentTaskComponent. This method can be used to initialize the form with default values when the component is first loaded, or to reset the form after a task has been added or updated.
    /// </summary> <returns>A task that represents the asynchronous operation.</returns>
    private void BuildEmptyTask()
    {
        _newTask = new TTask { CreatorUserId = StaticUserInfoBlazor.User!.Id, CreatorUser = new AppUser { Id = StaticUserInfoBlazor.User.Id, UserName = StaticUserInfoBlazor.User.UserName }, DueDateUtc = DateTime.UtcNow.AddDays(7), DepartmentId = StaticUserInfoBlazor.SelectedDepartment!.Id, Department = StaticUserInfoBlazor.SelectedDepartment!.Department };
    }

    /// <summary>
    /// Load data from the API after the component is initialized
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Load user info
        FormResult = new FormResult { Succeeded = true, ErrorList = ["Loading user info..."] };
        ShowSpinner = true;
        //_ = await AccountService.CheckAuthenticatedAsync();
        if (StaticUserInfoBlazor.User is null)
        {
            Navigation.NavigateTo("/");
            return;
        }
        BuildEmptyTask();
        
        if (StaticUserInfoBlazor.OrganizationRole == Shared.RolesEnum.OrganizationAdmin || StaticUserInfoBlazor.DepartmentRole == Shared.RolesEnum.EnterpriseAdmin)
        {
            var (usersWithAccessToOrganization, formResultUsersWithAccessToOrganization) = await DepartmentTaskService.GetUsersWithAccessToOrganizationAsync(StaticUserInfoBlazor.SelectedOrganization!.Id, CancellationToken.None);
            if (formResultUsersWithAccessToOrganization is not null)
                FormResult = formResultUsersWithAccessToOrganization;
            else if (usersWithAccessToOrganization is not null)
                UsersWithAccessToOrganization = usersWithAccessToOrganization;
        }

        var (usersWithAccessToDepartment, formResultUsersWithAccessToDepartment) = await DepartmentTaskService.GetUsersWithAccessToDepartmentAsync(StaticUserInfoBlazor.SelectedDepartment!.Id, CancellationToken.None);
        if (formResultUsersWithAccessToDepartment is not null)
            FormResult = formResultUsersWithAccessToDepartment;
        else if (usersWithAccessToDepartment is not null)
            UsersWithAccessToDepartment = usersWithAccessToDepartment;

        ShowSpinner = false;
        FormResult = null;
        await base.OnInitializedAsync();
    }

}
