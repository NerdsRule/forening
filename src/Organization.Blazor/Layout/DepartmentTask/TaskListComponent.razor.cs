
namespace Organization.Blazor.Layout.DepartmentTask;

partial class TaskListComponent
{
    private Dictionary<int, DepartmentTaskComponent> _taskComponents = [];
    private List<TTask> _tasks { get; set; } = [];
    private List<TDepartment> _departments { get; set; } = [];
    private FormResult? _taskResult;
    [Parameter] public List<UserModel> UsersWithAccess { get; set; } = [];
     [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private IDepartmentTaskService DepartmentTaskService { get; set; } = default!;

    /// <summary>
    /// Refresh the list of tasks by reloading them from the API. This method can be called after a task is added, updated, or deleted to ensure that the list of tasks displayed in the component is up to date with the latest data from the server.
    /// </summary> <returns>A task that represents the asynchronous operation.</returns>
    private async Task RefreshTasks()
    {        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
        var taskResponse = await DepartmentTaskService.GetOwnedTasksByDepartmentIdAsync(StaticUserInfoBlazor.SelectedDepartment!.DepartmentId, ct);
        if (taskResponse.data != null)        {
            _tasks = taskResponse.data;
        }
        else        {
            _taskResult = taskResponse.formResult;
        }
        StateHasChanged();
    }

    /// <summary>
    /// Add or update a task in the list of tasks. This method will be called when a task is added or updated in the DepartmentTaskComponent, and it will update the list of tasks accordingly. If the task already exists in the list, it will be updated with the new information. If the task does not exist in the list, it will be added to the list.
    /// </summary>
    /// <param name="task">The task to add or update.</param>
    public void AddTaskToList(TTask task)
    {
        var existingTaskIndex = _tasks.FindIndex(t => t.Id == task.Id);
        if (existingTaskIndex != -1)
        {
            _tasks[existingTaskIndex] = task;
        }
        else
        {
            _tasks.Add(task);
        }
        StateHasChanged();
    }

    ///<summary>
    /// Remove a task from the list of tasks. This method will be called when a task is deleted in the DepartmentTaskComponent, and it will remove the task from the list of tasks.
    /// </summary>
    /// <param name="taskId">The ID of the task to remove.</param>
    public void RemoveTaskFromList(int taskId)
    {
        var existingTaskIndex = _tasks.FindIndex(t => t.Id == taskId);
        if (existingTaskIndex != -1)        {
            _tasks.RemoveAt(existingTaskIndex);
            StateHasChanged();
        }
    }
    
    /// <summary>
    /// Load data from the API after the component is initialized
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        //_ = await AccountService.CheckAuthenticatedAsync();
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
        var response = await AccountService.GetDepartmentsByOrganizationIdAsync(StaticUserInfoBlazor.SelectedOrganization!.Id, StaticUserInfoBlazor.User!.Id, ct);
        if (response.departments != null)
        {
            _departments = response.departments;
        }
        else
        {
            _taskResult = response.formResult;
        }
        var taskResponse = await DepartmentTaskService.GetOwnedTasksByDepartmentIdAsync(StaticUserInfoBlazor.SelectedDepartment!.DepartmentId, ct);
        if (taskResponse.data != null)
        {
            _tasks = taskResponse.data;
        }
        else
        {
            _taskResult = taskResponse.formResult;
        }
        await base.OnInitializedAsync();
    }

    
   
}
