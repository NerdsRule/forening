namespace Organization.Blazor.Layout.UserBudget;

partial class MyBudgetComponent
{
    private readonly List<TUserBudget> _budgets = [];
    private List<string> _existingTags = [];
    private List<string> _descriptionSuggestions = [];
    private TUserBudget _newBudget = new();
    private string _newTagInput = string.Empty;
    private bool _isCollapsed;
    private bool _isLoadingBudget;
    private bool _hasLoadedBudget;
    private int? _departmentId;
    private string? _lastLoadedUserId;
    private int? _lastLoadedDepartmentId;
    private FormResultComponent _formResult = null!;

    public decimal Sum => _budgets.Sum(budget => budget.Amount);
    public decimal VisibleSum => GetVisibleBudgets().Sum(budget => budget.Amount);
    private bool CanModifyEntries =>
        UiStateService.HasRole(Shared.RolesEnum.BudgetAdministrator);

    [Parameter] public bool Collapsed { get; set; } = true;
    [Parameter] public bool CanEdit { get; set; } = true;
    [Parameter] public string? Title { get; set; }
    [Parameter] public string? UserId { get; set; }
    [Parameter] public EventCallback<decimal> SumChanged { get; set; }
    [Parameter] public EventCallback<(string? UserId, decimal BudgetSum, decimal UsedSum)> SummaryChanged { get; set; }
    [Parameter] public EventCallback<(string? UserId, bool IsVisible)> VisibilityChanged { get; set; }
    [Parameter] public EventCallback<bool> LoadingChanged { get; set; }
    [Parameter] public string? FilterTag { get; set; }
    [Parameter] public string? FilterDescription { get; set; }
    [Inject] private IUserBudgetService UserBudgetService { get; set; } = null!;
    [Inject] private IUiStateService UiStateService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _isCollapsed = Collapsed;
        _departmentId = ResolveDepartmentId();
        if (!_departmentId.HasValue)
            return;

        BuildNewBudget();
        await TryLoadBudgetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await TryLoadBudgetAsync();
    }

    protected override void OnParametersSet()
    {
        if (_isCollapsed != Collapsed)
        {
            _isCollapsed = Collapsed;
        }

        _ = NotifyParentStateChangedAsync();
    }

    private async Task LoadBudgetAsync()
    {
        await NotifyLoadingChangedAsync(true);
        _departmentId = ResolveDepartmentId();
        if (!_departmentId.HasValue)
            return;

        var targetUserId = ResolveUserId();
        if (string.IsNullOrWhiteSpace(targetUserId))
            return;

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        if (string.IsNullOrWhiteSpace(UserId) || string.Equals(UserId, UiStateService.User?.Id, StringComparison.OrdinalIgnoreCase))
        {
            var result = await UserBudgetService.GetMyBudgetAsync(_departmentId.Value, cancellationTokenSource.Token);
            if (result.data is not null)
            {
                _budgets.Clear();
                _budgets.AddRange(result.data);
                await SumChanged.InvokeAsync(Sum);
                await NotifySummaryChangedAsync();
            }
            else if (result.formResult is not null && _formResult is not null)
            {
                _formResult.SetFormResult(result.formResult, 2);
            }
        }
        else
        {
            var result = await UserBudgetService.GetUserBudgetAsync(_departmentId.Value, targetUserId, cancellationTokenSource.Token);
            if (result.data is not null)
            {
                _budgets.Clear();
                _budgets.AddRange(result.data);
                await SumChanged.InvokeAsync(Sum);
                await NotifySummaryChangedAsync();
            }
            else if (result.formResult is not null && _formResult is not null)
            {
                _formResult.SetFormResult(result.formResult, 2);
            }
        }

        await LoadSuggestionsAsync(string.Empty, cancellationTokenSource.Token);
        await InvokeAsync(StateHasChanged);
        await NotifyLoadingChangedAsync(false);
    }

    private async Task AddBudgetAsync()
    {
        if (!_departmentId.HasValue)
            return;

        _newBudget.DepartmentId = _departmentId.Value;
        _newBudget.AppUserId = ResolveUserId() ?? string.Empty;
        _newBudget.Tags = NormalizeTags(_newTagInput);

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var result = await UserBudgetService.AddUpdateBudgetAsync(_newBudget, cancellationTokenSource.Token);
        if (result.data is not null)
        {
            BuildNewBudget();
            await LoadBudgetAsync();
            _formResult.SetFormResult(new FormResult { Succeeded = true, ErrorList = ["Budget entry added"] }, 2);
            await InvokeAsync(StateHasChanged);
        }
        else if (result.formResult is not null)
        {
            _formResult.SetFormResult(result.formResult, 2);
        }
    }

    private async Task OnBudgetChangedAsync(TUserBudget budget)
    {
        if (_departmentId.HasValue)
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await LoadSuggestionsAsync(string.Empty, cancellationTokenSource.Token);
        }

        await LoadBudgetAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnBudgetDeletedAsync(int budgetId)
    {
        await LoadBudgetAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnNewDescriptionChanged(string value)
    {
        _newBudget.Description = value;
        if (!_departmentId.HasValue)
            return;

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await LoadDescriptionsAsync(value, cancellationTokenSource.Token);
    }

    private Task OnNewDescriptionSelected(string value)
    {
        _newBudget.Description = value;
        return Task.CompletedTask;
    }

    private Task OnNewTagChanged(string value)
    {
        _newTagInput = value;
        return Task.CompletedTask;
    }

    private Task OnNewTagSelected(string value)
    {
        _newTagInput = value;
        return Task.CompletedTask;
    }

    private async Task LoadSuggestionsAsync(string descriptionQuery, CancellationToken cancellationToken)
    {
        if (!_departmentId.HasValue)
            return;

        var tagsResult = await UserBudgetService.GetDistinctBudgetTagsByDepartmentIdAsync(_departmentId.Value, cancellationToken);
        if (tagsResult.data is not null)
            _existingTags = tagsResult.data;

        await LoadDescriptionsAsync(descriptionQuery, cancellationToken);
    }

    private async Task LoadDescriptionsAsync(string descriptionQuery, CancellationToken cancellationToken)
    {
        if (!_departmentId.HasValue)
            return;

        var descriptionsResult = await UserBudgetService.GetBudgetDescriptionSuggestionsAsync(_departmentId.Value, descriptionQuery, cancellationToken);
        if (descriptionsResult.data is not null)
            _descriptionSuggestions = descriptionsResult.data;
    }

    private void BuildNewBudget()
    {
        _newBudget = new TUserBudget
        {
            DepartmentId = _departmentId ?? 0,
            AppUserId = ResolveUserId() ?? string.Empty,
            Description = string.Empty
        };
        _newTagInput = string.Empty;
    }

    private async Task TryLoadBudgetAsync()
    {
        if (_isLoadingBudget)
            return;

        _departmentId = ResolveDepartmentId();
        var targetUserId = ResolveUserId();
        if (!_departmentId.HasValue || string.IsNullOrWhiteSpace(targetUserId))
            return;

        if (_hasLoadedBudget && _lastLoadedDepartmentId == _departmentId && string.Equals(_lastLoadedUserId, targetUserId, StringComparison.Ordinal))
            return;

        _isLoadingBudget = true;
        try
        {
            await LoadBudgetAsync();
            _hasLoadedBudget = true;
            _lastLoadedDepartmentId = _departmentId;
            _lastLoadedUserId = targetUserId;
            await InvokeAsync(StateHasChanged);
        }
        finally
        {
            _isLoadingBudget = false;
        }
    }

    private int? ResolveDepartmentId()
    {
        var selectedDepartmentId = UiStateService.SelectedDepartment?.DepartmentId;
        if (selectedDepartmentId.HasValue)
            return selectedDepartmentId.Value;

        return UiStateService.User?.AppUserDepartments.FirstOrDefault()?.DepartmentId;
    }

    private string? ResolveUserId()
        => string.IsNullOrWhiteSpace(UserId) ? UiStateService.User?.Id : UserId;

    private static List<string> NormalizeTags(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(tag => tag.ToLowerInvariant())
            .Where(tag => tag.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private IEnumerable<TUserBudget> GetVisibleBudgets()
    {
        var queryTag = FilterTag?.Trim();
        var queryDescription = FilterDescription?.Trim();

        return _budgets.Where(budget =>
        {
            var normalizedTags = budget.GetNormalizedTags();
            var matchesTag = string.IsNullOrWhiteSpace(queryTag)
                || normalizedTags.Any(tag => tag.Contains(queryTag, StringComparison.OrdinalIgnoreCase));
            var matchesDescription = string.IsNullOrWhiteSpace(queryDescription)
                || (budget.Description?.Contains(queryDescription, StringComparison.OrdinalIgnoreCase) ?? false);

            return matchesTag && matchesDescription;
        });
    }

    private Task NotifySummaryChangedAsync()
    {
        var budgetSum = GetVisibleBudgets().Where(b => b.Amount > 0).Sum(b => b.Amount);
        var usedSum = GetVisibleBudgets().Where(b => b.Amount < 0).Sum(b => b.Amount);

        return SummaryChanged.HasDelegate
            ? SummaryChanged.InvokeAsync((ResolveUserId(), budgetSum, usedSum))
            : Task.CompletedTask;
    }

    private Task NotifyLoadingChangedAsync(bool isLoading)
        => LoadingChanged.HasDelegate
            ? LoadingChanged.InvokeAsync(isLoading)
            : Task.CompletedTask;

    private Task NotifyVisibilityChangedAsync()
        => VisibilityChanged.HasDelegate
            ? VisibilityChanged.InvokeAsync((ResolveUserId(), GetVisibleBudgets().Any()))
            : Task.CompletedTask;

    private Task NotifyParentStateChangedAsync()
        => NotifySummaryChangedAsync().ContinueWith(_ => NotifyVisibilityChangedAsync(), TaskScheduler.Default);

    private void ToggleCollapsed()
    {
        _isCollapsed = !_isCollapsed;
        InvokeAsync(StateHasChanged);
    }

    private void OnHeaderKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Enter" || args.Key == " ")
        {
            ToggleCollapsed();
        }
    }
}