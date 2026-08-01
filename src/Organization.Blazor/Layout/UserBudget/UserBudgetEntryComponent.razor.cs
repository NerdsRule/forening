namespace Organization.Blazor.Layout.UserBudget;

partial class UserBudgetEntryComponent
{
    private bool _isEditing;
    private TUserBudget _editBudget = new();
    private string TagInput { get; set; } = string.Empty;

    [Parameter, EditorRequired] public TUserBudget Budget { get; set; } = null!;
    [Parameter] public bool CanEdit { get; set; }
    [Parameter] public IReadOnlyList<string> ExistingTags { get; set; } = [];
    [Parameter] public IReadOnlyList<string> DescriptionSuggestions { get; set; } = [];
    [Parameter] public EventCallback<TUserBudget> OnBudgetChanged { get; set; }
    [Parameter] public EventCallback<int> OnBudgetDeleted { get; set; }
    [Inject] private IUserBudgetService UserBudgetService { get; set; } = null!;

    private void StartEdit()
    {
        _editBudget = CloneBudget(Budget);
        TagInput = string.Join(", ", _editBudget.GetNormalizedTags());
        _isEditing = true;
    }

    private void CancelEdit()
    {
        _isEditing = false;
    }

    private Task OnDescriptionChanged(string value)
    {
        _editBudget.Description = value;
        return Task.CompletedTask;
    }

    private Task OnDescriptionSelected(string value)
    {
        _editBudget.Description = value;
        return Task.CompletedTask;
    }

    private Task OnTagChanged(string value)
    {
        TagInput = value;
        return Task.CompletedTask;
    }

    private Task OnTagSelected(string value)
    {
        TagInput = value;
        return Task.CompletedTask;
    }

    private async Task SaveAsync()
    {
        _editBudget.Tags = NormalizeTags(TagInput);
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var result = await UserBudgetService.AddUpdateBudgetAsync(_editBudget, cancellationTokenSource.Token);
        if (result.data is null)
            return;

        _isEditing = false;
        await OnBudgetChanged.InvokeAsync(result.data);
    }

    private async Task DeleteAsync()
    {
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var result = await UserBudgetService.DeleteBudgetAsync(Budget.Id, cancellationTokenSource.Token);
        if (!result.Succeeded)
            return;

        await OnBudgetDeleted.InvokeAsync(Budget.Id);
    }

    private static TUserBudget CloneBudget(TUserBudget budget)
        => new()
        {
            Id = budget.Id,
            AppUserId = budget.AppUserId,
            AppUser = budget.AppUser,
            DepartmentId = budget.DepartmentId,
            Department = budget.Department,
            Amount = budget.Amount,
            Description = budget.Description,
            Tags = budget.Tags.ToList()
        };

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
}