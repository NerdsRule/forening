
namespace Organization.Blazor.Layout.DepartmentTask;

partial class DepartmentTaskComponent
{
    private UserModel Me { get; set; } = StaticUserInfoBlazor.User!;
    private bool DisplayDetails { get; set; } = false;
    private bool DisableSubmit { get; set; } = false;
    private bool DisableDelete { get; set; } = true;
    private bool ShowSpinner { get; set; } = false;
    private FormResult? FormResult { get; set; } = null;
    private string AddUpdateText => ChildContent.Id == 0 ? "Add Task" : "Update Task";
    
    // Computed property for checkbox binding
    private bool IsAssignedToMe 
    {
        get => ChildContent?.AssignedUserId == StaticUserInfoBlazor.User?.Id;
        set 
        {
            if (value)
            {
                ChildContent.AssignedUserId = StaticUserInfoBlazor.User!.Id;
                ChildContent.AssignedUser = new AppUser { Id = StaticUserInfoBlazor.User.Id, UserName = StaticUserInfoBlazor.User.UserName };
            }
            else
            {
                ChildContent.AssignedUserId = null;
                ChildContent.AssignedUser = null;
            }
            StateHasChanged(); // Trigger re-render
        }
    }
    [Parameter] public bool InitDisplayDetails { get; set; } = false;
    [Parameter] public TTask ChildContent { get; set; } = null!;
    [Parameter] public EventCallback<TTask> OnTaskAddedOrUpdatedEvent { get; set; }
    [Parameter] public EventCallback<int> OnTaskDeletedEvent { get; set; }
    [Parameter] public List<UserModel> UsersWithAccess { get; set; } = [];
    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private IDepartmentTaskService DepartmentTaskService { get; set; } = null!;

    /// <summary>
    /// Handle the event when a task is added or updated in the DepartmentTaskComponent. This method will be called with the task that was added or updated, and it can be used to perform any necessary actions, such as displaying a success message or refreshing a list of tasks.
    /// </summary>
    /// <param name="task">The task that was added or updated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task OnTaskAddedOrUpdated(TTask task)
    {
        DisableSubmit = true;
        ShowSpinner = true;
        FormResult = null;
        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var _updateResult = await DepartmentTaskService.AddUpdateTaskAsync(task, cts.Token);
        ShowSpinner = false;
        DisableSubmit = false;
        if (_updateResult.formResult != null && !_updateResult.formResult.Succeeded)
        {
            FormResult = _updateResult.formResult;
            return;
        }
        else if (_updateResult.data != null)
        {
            task = _updateResult.data;
            FormResult = new FormResult { Succeeded = true, ErrorList = ["Task added/updated successfully!"] };
        }
        await OnTaskAddedOrUpdatedEvent.InvokeAsync(task);
    }

    /// <summary>
    /// Set user assigned to the task if user is from list and "" could come from selected component in the form. This method will be called when the user selects a user from the dropdown, and it will update the AssignedUserId and AssignedUser properties of the task accordingly.
    /// </summary>
    /// <param name="userId">The ID of the user that was selected from the dropdown. This can be an integer representing the user's ID, or it can be an empty string if the user selects the option to unassign the task.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private void SetAssignedUser(string userId)
    {        
        if (string.IsNullOrEmpty(userId))
        {
            ChildContent.AssignedUserId = null;
            ChildContent.AssignedUser = null;
        }
        else
        {
            var selectedUser = UsersWithAccess.FirstOrDefault(u => u.Id == userId);
            ChildContent.AssignedUserId = userId;
            if (selectedUser != null)            
            {
                ChildContent.AssignedUser = new AppUser { Id = selectedUser.Id, UserName = selectedUser.UserName };
            }
        }
        StateHasChanged(); // Trigger re-render to update the dropdown selection
    }

    /// <summary>
    /// Delete the task. This method will be called when the delete button is clicked, and it will delete the task from the database and update the parent component accordingly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task DeleteTaskAsync()
    {
        if (ChildContent.Id == 0)
        {
            FormResult = new FormResult { Succeeded = false, ErrorList = ["Cannot delete a task that has not been saved yet."] };
            return;
        }
        ShowSpinner = true;
        FormResult = null;
        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var _deleteResult = await DepartmentTaskService.DeleteTaskAsync(ChildContent.Id, cts.Token);
        ShowSpinner = false;
        if (_deleteResult != null && _deleteResult.Succeeded)
        {
            await OnTaskDeletedEvent.InvokeAsync(ChildContent.Id);
        }
        else
        {
            FormResult = _deleteResult ?? new FormResult { Succeeded = false, ErrorList = ["An error occurred while trying to delete the task."] };
        }
    }



    protected override async Task OnInitializedAsync()
    {
        DisplayDetails = InitDisplayDetails;
        DisableDelete = !(StaticUserInfoBlazor.DepartmentRole == Shared.RolesEnum.DepartmentAdmin || StaticUserInfoBlazor.OrganizationRole == Shared.RolesEnum.OrganizationAdmin);
        await base.OnInitializedAsync();
    }
}
