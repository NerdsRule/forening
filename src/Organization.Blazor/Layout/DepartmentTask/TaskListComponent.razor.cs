
namespace Organization.Blazor.Layout.DepartmentTask;

partial class TaskListComponent
{
    private List<TTask> _tasks { get; set; } = [];
    private List<TDepartment> _departments { get; set; } = [];
    private FormResult? _taskResult;
    
    /// <summary>
    /// Load data from the API after the component is initialized
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        _ = await AccountService.CheckAuthenticatedAsync();
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
        var selectedDepartment = StaticUserInfoBlazor.SelectedDepartment?.Department;
        var selectedUser = StaticUserInfoBlazor.User;
        var response = await AccountService.GetDepartmentsByOrganizationIdAsync(StaticUserInfoBlazor.SelectedOrganization!.Id, StaticUserInfoBlazor.User!.Id, ct);
        if (response.departments != null)
        {
            _departments = response.departments;
        }
        else
        {
            _taskResult = response.formResult;
        }
        var taskResponse = await DepartmentTaskService.GetOwnedTasksByDepartmentIdAsync(StaticUserInfoBlazor.SelectedDepartment!.Id, ct);
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

    
    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private IDepartmentTaskService DepartmentTaskService { get; set; } = default!;
}
