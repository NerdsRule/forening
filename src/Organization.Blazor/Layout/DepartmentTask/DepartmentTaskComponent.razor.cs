
using System.Diagnostics;

namespace Organization.Blazor.Layout.DepartmentTask;

partial class DepartmentTaskComponent
{
    private FormResultComponent FormResultComponent { get; set; } = null!;
    private UserModel Me { get; set; } = StaticUserInfoBlazor.User!;
    private bool DisplayDetails { get; set; } = false;
    private bool DisableSubmit { get; set; } = false;
    private bool DisableDelete { get; set; } = true;
    private bool DisableName { get; set; } = false;
    private bool DisableDescription { get; set; } = false;
    private bool DisableEstimatedTimeMinutes { get; set; } = false;
    private bool DisableDueDateUtc { get; set; } = false;
    private bool DisablePointsAwarded { get; set; } = false;
    private bool DisableAssignedUser { get; set; } = false;
    private bool DisableTags { get; set; } = false;
    private bool DisableIsAssignedToMe { get; set; } = false;
    private bool DisableStatus { get; set; } = false;
    private bool ShowSpinner { get; set; } = false;
    private bool TagsLoaded { get; set; } = false;
    private string TagInput { get; set; } = string.Empty;
    private string AssignedUserSearchText { get; set; } = string.Empty;
    private List<string> ExistingDepartmentTags { get; set; } = [];
    private bool IsDepartmentAdmin { get; set; } = StaticUserInfoBlazor.DepartmentRole == Shared.RolesEnum.DepartmentAdmin;
    private bool IsOrganizationAdmin { get; set; } = StaticUserInfoBlazor.OrganizationRole == Shared.RolesEnum.OrganizationAdmin;
    private bool IsEnterpriseAdmin { get; set; } = StaticUserInfoBlazor.OrganizationRole == Shared.RolesEnum.EnterpriseAdmin;
    private bool IsDepartmentMember { get; set; } = StaticUserInfoBlazor.DepartmentRole == Shared.RolesEnum.DepartmentMember;
    private string AddUpdateText => ChildContent.Id == 0 ? "Add Task" : "Update Task";
    private bool IsThisTaskAssignedToMe => ChildContent.AssignedUserId == StaticUserInfoBlazor.User?.Id;
    private bool IsNotAssigned => ChildContent.AssignedUserId == null;
    private (string text, Shared.TaskStatusEnum enumValue)[] StatusOptions => TaskWorkflows.GetAvailableTaskStatus(ChildContent.Status, [StaticUserInfoBlazor.DepartmentRole, StaticUserInfoBlazor.OrganizationRole], IsThisTaskAssignedToMe);
    
    [Parameter] public bool InitDisplayDetails { get; set; } = false;
    [Parameter] public TTask ChildContent { get; set; } = null!;
    [Parameter] public EventCallback<TTask> OnTaskAddedOrUpdatedEvent { get; set; }
    [Parameter] public EventCallback<int> OnTaskDeletedEvent { get; set; }
    [Parameter] public List<UserModel> UsersWithAccess { get; set; } = [];
    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private IDepartmentTaskService DepartmentTaskService { get; set; } = null!;
    [Inject] private IUiStateService UiStateService { get; set; } = null!;

    /// <summary>
    /// Set Disable properties based on user permissions and whether the task is new or existing. This method will be called from the parent component to set the appropriate disable states for the form fields and buttons based on the current user's permissions and whether they are creating a new task or editing an existing one.
    /// </summary>
    private void SetDisableProperties()
    {
        if (ChildContent.Status == Shared.TaskStatusEnum.VerifiedCompleted && !IsDepartmentAdmin && !IsOrganizationAdmin && !IsEnterpriseAdmin)
        {
            DisableName = true;
            DisableDescription = true;
            DisableEstimatedTimeMinutes = true;
            DisableDueDateUtc = true;
            DisablePointsAwarded = true;
            DisableAssignedUser = true;
            DisableTags = true;
            DisableIsAssignedToMe = true;
            DisableSubmit = true;
            DisableDelete = true;
            DisableStatus = true;
            return;
        }
        bool isNewTask = ChildContent.Id == 0;
        
        DisableName = !IsDepartmentAdmin && !IsOrganizationAdmin && !IsEnterpriseAdmin;
        DisableDescription = !IsDepartmentAdmin && !IsOrganizationAdmin && !IsEnterpriseAdmin;
        DisableEstimatedTimeMinutes = !IsDepartmentAdmin && !IsOrganizationAdmin && !IsEnterpriseAdmin;
        DisableDueDateUtc = !IsDepartmentAdmin && !IsOrganizationAdmin && !IsEnterpriseAdmin;
        DisablePointsAwarded = !IsDepartmentAdmin && !IsOrganizationAdmin && !IsEnterpriseAdmin;
        DisableAssignedUser = !IsDepartmentAdmin && !IsOrganizationAdmin && !IsEnterpriseAdmin;
        DisableTags = !IsDepartmentAdmin && !IsOrganizationAdmin && !IsEnterpriseAdmin;
        if (isNewTask || ChildContent.Status == Shared.TaskStatusEnum.VerifiedCompleted || ChildContent.Status == Shared.TaskStatusEnum.Rejected)
        {
            DisableIsAssignedToMe = true;
        }
        else if (IsThisTaskAssignedToMe || IsDepartmentAdmin || IsOrganizationAdmin || IsEnterpriseAdmin || IsNotAssigned)
        {
            DisableIsAssignedToMe = false;
        }
        else
        {
            DisableIsAssignedToMe = true;
        }
        DisableDelete = !IsDepartmentAdmin && !IsOrganizationAdmin && !IsEnterpriseAdmin;
        DisableStatus = (ChildContent.Status == Shared.TaskStatusEnum.VerifiedCompleted && IsThisTaskAssignedToMe) || (!IsDepartmentAdmin && !IsOrganizationAdmin && !IsEnterpriseAdmin && !IsAssignedToMe);
        if (isNewTask)
            DisableSubmit = false;
        else if (DisableName && DisableDescription && DisableEstimatedTimeMinutes && DisableDueDateUtc && DisablePointsAwarded && DisableStatus && DisableIsAssignedToMe)
            DisableSubmit = true;
        else
            DisableSubmit = false;
    }

    /// <summary>
    /// Get or set whether the task is assigned to the current user. 
    /// This property will be used to bind the value of the "Assign to me" checkbox in the form, and it will update the AssignedUserId and AssignedUser properties of the task accordingly when the checkbox is toggled.
    /// </summary>
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

    /// <summary>
    /// Handle the event when a task is added or updated in the DepartmentTaskComponent. This method will be called with the task that was added or updated, and it can be used to perform any necessary actions, such as displaying a success message or refreshing a list of tasks.
    /// </summary>
    /// <param name="task">The task that was added or updated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task OnTaskAddedOrUpdated(TTask task)
    {
        DisableSubmit = true;
        ShowSpinner = true;
        FormResultComponent.ClearFormResult();
        task.Tags = NormalizeTags(task.Tags);
        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var _updateResult = await DepartmentTaskService.AddUpdateTaskAsync(task, cts.Token);
        ShowSpinner = false;
        DisableSubmit = false;
        if (_updateResult.formResult != null && !_updateResult.formResult.Succeeded)
        {
            FormResultComponent.SetFormResult(_updateResult.formResult);
            return;
        }
        else if (_updateResult.data != null)
        {
            task = _updateResult.data;
            await UpdateAwardedPointsForUserAsync();
            FormResultComponent.SetFormResult(new FormResult { Succeeded = true, ErrorList = ["Task added/updated successfully!"] }, 2);
        }
        await OnTaskAddedOrUpdatedEvent.InvokeAsync(task);
    }

    private static List<string> NormalizeTags(List<string>? tags)
    {
        if (tags is null || tags.Count == 0)
            return [];

        return tags
            .Select(tag => tag?.Trim().ToLowerInvariant())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct()
            .Cast<string>()
            .ToList();
    }

    private Task OnTagInputChanged(string value)
    {
        TagInput = value;
        return Task.CompletedTask;
    }

    private async Task OnTagSelected(string selectedTag)
    {
        TagInput = selectedTag;
        await AddTagFromInputAsync();
    }

    private async Task AddTagFromInputAsync()
    {
        if (DisableTags)
            return;

        var normalizedTag = TagInput.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedTag))
            return;

        ChildContent.Tags ??= [];
        if (!ChildContent.Tags.Any(tag => string.Equals(tag, normalizedTag, StringComparison.OrdinalIgnoreCase)))
        {
            ChildContent.Tags.Add(normalizedTag);
            ChildContent.Tags = NormalizeTags(ChildContent.Tags);
        }

        TagInput = string.Empty;
        await EnsureTagsLoadedAsync();
        StateHasChanged();
    }

    private void RemoveTag(string tag)
    {
        if (DisableTags || ChildContent.Tags is null)
            return;

        ChildContent.Tags = ChildContent.Tags
            .Where(current => !string.Equals(current, tag, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task EnsureTagsLoadedAsync()
    {
        if (TagsLoaded || ChildContent.DepartmentId <= 0)
            return;

        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var result = await DepartmentTaskService.GetDistinctTaskTagsByDepartmentIdAsync(ChildContent.DepartmentId, cts.Token);
        ExistingDepartmentTags = result.data ?? [];
        TagsLoaded = true;
    }

    private async Task ToggleDetailsAsync()
    {
        DisplayDetails = !DisplayDetails;
        if (DisplayDetails && !DisableTags)
            await EnsureTagsLoadedAsync();
    }

    private static string GetUserDisplayText(UserModel user)
        => $"{user.DisplayName} ({user.UserName})";

    private Task OnAssignedUserSearchChanged(string value)
    {
        AssignedUserSearchText = value;
        return Task.CompletedTask;
    }

    private async Task OnAssignedUserSelected(string selectedText)
    {
        var selected = UsersWithAccess.FirstOrDefault(user =>
            string.Equals(GetUserDisplayText(user), selectedText, StringComparison.OrdinalIgnoreCase));

        if (selected is null)
            return;

        await SetAssignedUser(selected.Id);
    }

    private async Task ClearAssignedUserAsync()
    {
        if (DisableAssignedUser)
            return;

        await SetAssignedUser(string.Empty);
    }

    /// <summary>
    /// Set user assigned to the task if user is from list and "" could come from selected component in the form. This method will be called when the user selects a user from the dropdown, and it will update the AssignedUserId and AssignedUser properties of the task accordingly.
    /// </summary>
    /// <param name="userId">The ID of the user that was selected from the dropdown. This can be an integer representing the user's ID, or it can be an empty string if the user selects the option to unassign the task.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SetAssignedUser(string userId)
    {
        FormResultComponent.ClearFormResult();
        if (string.IsNullOrEmpty(userId))
        {
            ChildContent.AssignedUserId = null;
            ChildContent.AssignedUser = null;
            AssignedUserSearchText = string.Empty;
        }
        else
        {
            var selectedUser = UsersWithAccess.FirstOrDefault(u => u.Id == userId);
            ChildContent.AssignedUserId = userId;
            if (selectedUser != null)            
            {
                ChildContent.AssignedUser = new AppUser { Id = selectedUser.Id, UserName = selectedUser.UserName };
                AssignedUserSearchText = GetUserDisplayText(selectedUser);
                await UpdateAwardedPointsForUserAsync();
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
            FormResultComponent.SetFormResult(new FormResult { Succeeded = false, ErrorList = ["Cannot delete a task that has not been saved yet."] });
            return;
        }
        ShowSpinner = true;
        FormResultComponent.ClearFormResult();
        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var _deleteResult = await DepartmentTaskService.DeleteTaskAsync(ChildContent.Id, cts.Token);
        ShowSpinner = false;
        if (_deleteResult != null && _deleteResult.Succeeded)
        {
            await OnTaskDeletedEvent.InvokeAsync(ChildContent.Id);
            await UpdateAwardedPointsForUserAsync();
        }
        else
        {
            FormResultComponent.SetFormResult(_deleteResult ?? new FormResult { Succeeded = false, ErrorList = ["An error occurred while trying to delete the task."] });
        }
    }

    /// <summary>
    /// Refresh user points from the canonical user-info endpoint.
    /// </summary>
    private async Task UpdateAwardedPointsForUserAsync()
    {
        if (StaticUserInfoBlazor.User is null)
        {
            Debug.WriteLine("No user is currently logged in.");
            return;
        }

        _ = await AccountService.CheckAuthenticatedAsync();
        UiStateService.NotifyUserUpdated();
    }

    /// <summary>
    /// OnInitializedAsync is called when the component is initialized. It sets the DisplayDetails property based on the InitDisplayDetails parameter and calls SetDisableProperties to set the appropriate disable states for the form fields and buttons based on the current user's permissions and whether they are creating a new task or editing an existing one. This ensures that the form is displayed correctly based on the context in which it is being used.
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        ChildContent.Tags ??= [];
        ChildContent.Tags = NormalizeTags(ChildContent.Tags);
        if (!string.IsNullOrEmpty(ChildContent.AssignedUserId))
        {
            var selectedUser = UsersWithAccess.FirstOrDefault(user => user.Id == ChildContent.AssignedUserId);
            if (selectedUser != null)
                AssignedUserSearchText = GetUserDisplayText(selectedUser);
            else
                AssignedUserSearchText = string.Empty;
        }
        DisplayDetails = InitDisplayDetails;
        SetDisableProperties();

        if (DisplayDetails && !DisableTags)
            await EnsureTagsLoadedAsync();

        await base.OnInitializedAsync();
    }
}
