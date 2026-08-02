
namespace Organization.Blazor.Layout.DepartmentTask;

partial class TaskListComponent
{
    private Dictionary<int, DepartmentTaskComponent> _taskComponents = [];
    private List<TTask> _tasks { get; set; } = [];
    private List<string> _existingDepartmentTags { get; set; } = [];
    private string _tagFilterInput { get; set; } = string.Empty;
    private string _assignedUserFilterSearchText { get; set; } = string.Empty;
    private string? _lastAssignedUserIdFilter { get; set; }

    private IEnumerable<TTask> FilterByStatusAndDate()
    {
        var query = _tasks.Where(t => (t.Status == Shared.TaskStatusEnum.VerifiedCompleted && ShowVerifiedCompleted) ||
                                      (t.Status == Shared.TaskStatusEnum.Rejected && ShowRejected) ||
                                      (t.Status == Shared.TaskStatusEnum.Completed && ShowCompleted) ||
                                      (t.Status == Shared.TaskStatusEnum.InProgress && ShowInProgress) ||
                                      (t.Status == Shared.TaskStatusEnum.NotStarted && ShowNotStarted));

        if (FilterUtcDate.HasValue)
            query = query.Where(t => t.DueDateUtc.Date >= FilterUtcDate.Value.Date);

        if (SelectedTagFilters.Count > 0)
        {
            query = query.Where(t => t.Tags != null && t.Tags.Any(tag =>
                SelectedTagFilters.Any(filter => string.Equals(filter, tag, StringComparison.OrdinalIgnoreCase))));
        }

        if (!string.IsNullOrWhiteSpace(AssignedUserIdFilter))
        {
            query = query.Where(t => t.AssignedUserId == AssignedUserIdFilter);
        }
        else if (!string.IsNullOrWhiteSpace(_assignedUserFilterSearchText))
        {
            query = query.Where(TaskMatchesAssignedUserFilter);
        }

        return query.OrderByDescending(t => t.DueDateUtc);
    }

    private List<TTask> _sortedAndFilteredTasks => 
        [.. FilterByStatusAndDate()];
    private List<AppUser> _assignedUsersInTasks =>
        [.. _tasks
            .Select(task => task.AssignedUser)
            .Where(user => user != null && !string.IsNullOrWhiteSpace(user.Id))
            .Cast<AppUser>()
            .DistinctBy(user => user.Id)
            .OrderBy(GetUserDisplayText)];
            private int? _loadedDepartmentId { get; set; }
    private FormResultComponent _taskResult {get;set;} = null!;
    [Parameter] public List<UserModel> UsersWithAccess { get; set; } = [];
    [Parameter] public bool ShowFilter { get; set; } = false;
    [Parameter] public DateTime? FilterUtcDate { get; set; }
    [Parameter] public List<string> SelectedTagFilters { get; set; } = [];
    [Parameter] public bool ShowVerifiedCompleted { get; set; } = false;
    [Parameter] public bool ShowRejected { get; set; } = false;
    [Parameter] public bool ShowCompleted { get; set; } = false;
    [Parameter] public bool ShowInProgress { get; set; } = true;
    [Parameter] public bool ShowNotStarted { get; set; } = true;
    [Parameter] public string? AssignedUserIdFilter { get; set; }
    [Parameter] public EventCallback<string?> AssignedUserIdFilterChanged { get; set; }
    [Inject] private IDepartmentTaskService DepartmentTaskService { get; set; } = default!;
    [Inject] private IUiStateService UiStateService { get; set; } = default!;

    private static string GetUserDisplayText(AppUser user)
    {
        if (string.IsNullOrWhiteSpace(user.DisplayName))
            return user.UserName ?? string.Empty;

        return string.IsNullOrWhiteSpace(user.UserName)
            ? user.DisplayName
            : $"{user.DisplayName} ({user.UserName})";
    }

    private static string GetUserDisplayText(UserModel user)
        => $"{user.DisplayName} ({user.UserName})";

    protected override void OnParametersSet()
    {
        if (AssignedUserIdFilter == _lastAssignedUserIdFilter)
            return;

        _lastAssignedUserIdFilter = AssignedUserIdFilter;
        var selectedUser = _assignedUsersInTasks.FirstOrDefault(user => user.Id == AssignedUserIdFilter);
        _assignedUserFilterSearchText = selectedUser is null ? string.Empty : GetUserDisplayText(selectedUser);
    }

    /// <summary>
    /// Refresh the list of tasks by reloading them from the API. This method can be called after a task is added, updated, or deleted to ensure that the list of tasks displayed in the component is up to date with the latest data from the server.
    /// </summary> <returns>A task that represents the asynchronous operation.</returns>
    private async Task RefreshTasks()
    {
        await LoadTasksForSelectedDepartmentAsync(forceReload: true);
    }

    private async Task LoadTasksForSelectedDepartmentAsync(bool forceReload = false)
    {
        var departmentId = UiStateService.SelectedDepartment?.DepartmentId;
        if (!departmentId.HasValue)
            return;

        if (!forceReload && _loadedDepartmentId == departmentId.Value)
            return;

        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
        var taskResponse = await DepartmentTaskService.GetOwnedTasksByDepartmentIdAsync(departmentId.Value, ct);
        if (taskResponse.data != null)        {
            _tasks = taskResponse.data;
            _loadedDepartmentId = departmentId.Value;
        }
        else if (taskResponse.formResult != null)
        {
            _taskResult.SetFormResult(taskResponse.formResult,2);
        }

        await LoadDepartmentTagsAsync(departmentId.Value, ct);
        StateHasChanged();
    }

    private static string NormalizeTag(string value) => value.Trim().ToLowerInvariant();

    private Task OnTagFilterInputChanged(string value)
    {
        _tagFilterInput = value;
        return Task.CompletedTask;
    }

    private async Task OnTagFilterSelected(string selectedTag)
    {
        _tagFilterInput = selectedTag;
        await AddTagFilterFromInputAsync();
    }

    private Task AddTagFilterFromInputAsync()
    {
        var normalized = NormalizeTag(_tagFilterInput);
        if (!string.IsNullOrWhiteSpace(normalized) &&
            !SelectedTagFilters.Any(tag => string.Equals(tag, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            SelectedTagFilters.Add(normalized);
        }

        _tagFilterInput = string.Empty;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private void RemoveTagFilter(string tag)
    {
        SelectedTagFilters = SelectedTagFilters
            .Where(existing => !string.Equals(existing, tag, StringComparison.OrdinalIgnoreCase))
            .ToList();
        StateHasChanged();
    }

    private void ClearTagFilters()
    {
        SelectedTagFilters = [];
        _tagFilterInput = string.Empty;
        StateHasChanged();
    }

    private Task OnAssignedUserFilterSearchChanged(string value)
    {
        _assignedUserFilterSearchText = value;
        AssignedUserIdFilter = null;
        return Task.CompletedTask;
    }

    private bool TaskMatchesAssignedUserFilter(TTask task)
    {
        var filterText = _assignedUserFilterSearchText.Trim();
        if (filterText.Length == 0)
            return true;

        if (task.AssignedUser != null &&
            (GetUserDisplayText(task.AssignedUser).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
             (task.AssignedUser.DisplayName?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
             (task.AssignedUser.UserName?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false)))
        {
            return true;
        }

        return false;
    }

    private async Task OnAssignedUserFilterSelected(string selectedText)
    {
        var selected = _assignedUsersInTasks.FirstOrDefault(user =>
            string.Equals(GetUserDisplayText(user), selectedText, StringComparison.OrdinalIgnoreCase));

        if (selected is null)
            return;

        AssignedUserIdFilter = selected.Id;
        _lastAssignedUserIdFilter = AssignedUserIdFilter;
        _assignedUserFilterSearchText = GetUserDisplayText(selected);
        await AssignedUserIdFilterChanged.InvokeAsync(AssignedUserIdFilter);
    }

    private async Task ClearAssignedUserFilterAsync()
    {
        AssignedUserIdFilter = null;
        _lastAssignedUserIdFilter = AssignedUserIdFilter;
        _assignedUserFilterSearchText = string.Empty;
        await AssignedUserIdFilterChanged.InvokeAsync(AssignedUserIdFilter);
    }

    private async Task LoadDepartmentTagsAsync(int departmentId, CancellationToken ct)
    {
        var tagsResponse = await DepartmentTaskService.GetDistinctTaskTagsByDepartmentIdAsync(departmentId, ct);
        if (tagsResponse.data != null)
        {
            _existingDepartmentTags = tagsResponse.data;
        }
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
        await LoadTasksForSelectedDepartmentAsync();
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadTasksForSelectedDepartmentAsync();
    }

    
   
}
