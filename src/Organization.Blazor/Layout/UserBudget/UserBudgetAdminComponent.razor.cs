namespace Organization.Blazor.Layout.UserBudget;

partial class UserBudgetAdminComponent
{
    private readonly List<UserModel> _users = [];
    private readonly Dictionary<string, decimal> _userBudgetSums = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, decimal> _userUsedSums = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _userVisibility = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _existingTags = [];
    private readonly List<string> _descriptionSuggestions = [];
    private string _userFilterText = string.Empty;
    private string _tagFilterText = string.Empty;
    private string _descriptionFilterText = string.Empty;
    private bool _isLoadingBudgets;
    private int? _departmentId;

    [Inject] private IDepartmentTaskService DepartmentTaskService { get; set; } = null!;
    [Inject] private IUserBudgetService UserBudgetService { get; set; } = null!;
    [Inject] private IUiStateService UiStateService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        _departmentId = UiStateService.SelectedDepartment?.DepartmentId;
        if (!_departmentId.HasValue)
            return;

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var usersResult = await DepartmentTaskService.GetUsersWithAccessToDepartmentAsync(_departmentId.Value, cancellationTokenSource.Token);
        if (usersResult.data is not null)
        {
            _users.Clear();
            _users.AddRange(usersResult.data);
        }

        var tagsResult = await UserBudgetService.GetDistinctBudgetTagsByDepartmentIdAsync(_departmentId.Value, cancellationTokenSource.Token);
        if (tagsResult.data is not null)
        {
            _existingTags.Clear();
            _existingTags.AddRange(tagsResult.data);
        }

        var descriptionsResult = await UserBudgetService.GetBudgetDescriptionSuggestionsAsync(_departmentId.Value, string.Empty, cancellationTokenSource.Token);
        if (descriptionsResult.data is not null)
        {
            _descriptionSuggestions.Clear();
            _descriptionSuggestions.AddRange(descriptionsResult.data);
        }
    }

    private List<UserModel> GetVisibleUsers()
    {
        var users = _users.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(_userFilterText))
        {
            var query = _userFilterText.Trim();
            users = users.Where(user =>
                (user.DisplayName is not null && user.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (user.UserName is not null && user.UserName.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(_tagFilterText) || !string.IsNullOrWhiteSpace(_descriptionFilterText))
        {
            users = users.Where(user =>
                !string.IsNullOrWhiteSpace(user.Id) &&
                _userVisibility.TryGetValue(user.Id, out var isVisible) && isVisible);
        }

        return users.ToList();
    }

    private decimal GetFilteredBudgetSum()
        => GetVisibleUsers()
            .Where(user => !string.IsNullOrWhiteSpace(user.Id) && _userBudgetSums.TryGetValue(user.Id, out _))
            .Sum(user => _userBudgetSums.TryGetValue(user.Id, out var sum) ? sum : 0m);

    private decimal GetFilteredUsedSum()
        => GetVisibleUsers()
            .Where(user => !string.IsNullOrWhiteSpace(user.Id) && _userUsedSums.TryGetValue(user.Id, out _))
            .Sum(user => _userUsedSums.TryGetValue(user.Id, out var sum) ? sum : 0m);

    private string GetFilteredBudgetDisplay()
        => $"{GetFilteredBudgetSum():N2} DKK";

    private string GetFilteredUsedDisplay()
        => $"{GetFilteredUsedSum():N2} DKK";

    private Task OnUserSummaryChanged((string? UserId, decimal BudgetSum, decimal UsedSum) summary)
    {
        if (string.IsNullOrWhiteSpace(summary.UserId))
            return Task.CompletedTask;

        _userBudgetSums[summary.UserId] = summary.BudgetSum;
        _userUsedSums[summary.UserId] = summary.UsedSum;
        _isLoadingBudgets = false;
        return InvokeAsync(StateHasChanged);
    }

    private Task OnUserVisibilityChanged((string? UserId, bool IsVisible) visibility)
    {
        if (string.IsNullOrWhiteSpace(visibility.UserId))
            return Task.CompletedTask;

        _userVisibility[visibility.UserId] = visibility.IsVisible;
        _isLoadingBudgets = false;
        return InvokeAsync(StateHasChanged);
    }

    private Task OnUserLoadingChanged(bool isLoading)
    {
        _isLoadingBudgets = isLoading;
        return InvokeAsync(StateHasChanged);
    }

    private Task OnTagFilterChanged(string value)
    {
        _tagFilterText = value;
        return Task.CompletedTask;
    }

    private Task OnTagFilterSelected(string value)
    {
        _tagFilterText = value;
        return Task.CompletedTask;
    }

    private Task OnDescriptionFilterChanged(string value)
    {
        _descriptionFilterText = value;
        return Task.CompletedTask;
    }

    private Task OnDescriptionFilterSelected(string value)
    {
        _descriptionFilterText = value;
        return Task.CompletedTask;
    }

    private static string GetUserDisplayText(UserModel user)
        => string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : $"{user.DisplayName} ({user.UserName})";
}