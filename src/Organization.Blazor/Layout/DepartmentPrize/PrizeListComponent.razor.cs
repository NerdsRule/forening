namespace Organization.Blazor.Layout.DepartmentPrize;

partial class PrizeListComponent
{
    private Dictionary<int, DepartmentPrizeComponent> _prizeComponents = [];
    private List<TPrize> _prizes { get; set; } = [];
    private List<TPrize> _sortedAndFilteredPrizes => [.. _prizes.OrderBy(p => p.PointsCost).ThenBy(p => p.Name)];
    private FormResultComponent _prizeResult { get; set; } = null!;

    [Parameter] public List<UserModel> UsersWithAccess { get; set; } = [];

    [Inject] private IPrizeService PrizeService { get; set; } = default!;

    private static bool HasPrizeReadAccess =>
        StaticUserInfoBlazor.DepartmentRole == Shared.RolesEnum.DepartmentAdmin ||
        StaticUserInfoBlazor.DepartmentRole == Shared.RolesEnum.DepartmentMember ||
        StaticUserInfoBlazor.OrganizationRole == Shared.RolesEnum.OrganizationAdmin ||
        StaticUserInfoBlazor.OrganizationRole == Shared.RolesEnum.EnterpriseAdmin;

    /// <summary>
    /// Refresh prizes for the selected department.
    /// </summary>
    private async Task RefreshPrizes()
    {
        if (!HasPrizeReadAccess)
        {
            // Endpoint requires admin-level role; avoid forbidden calls on home page for regular members.
            _prizes = [];
            StateHasChanged();
            return;
        }

        if (StaticUserInfoBlazor.SelectedDepartment is null)
        {
            _prizeResult.SetFormResult(new FormResult { Succeeded = false, ErrorList = ["No department selected."] }, 2);
            return;
        }

        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
        var response = await PrizeService.GetPrizesByDepartmentIdAsync(StaticUserInfoBlazor.SelectedDepartment.DepartmentId, ct);
        if (response.data != null)
        {
            _prizes = response.data;
        }
        else if (response.formResult != null)
        {
            _prizeResult.SetFormResult(response.formResult, 2);
        }

        StateHasChanged();
    }

    /// <summary>
    /// Add or update a prize in the current list.
    /// </summary>
    /// <param name="prize">Prize entity to add or update.</param>
    public void AddPrizeToList(TPrize prize)
    {
        var existingPrizeIndex = _prizes.FindIndex(p => p.Id == prize.Id);
        if (existingPrizeIndex != -1)
        {
            _prizes[existingPrizeIndex] = prize;
        }
        else
        {
            _prizes.Add(prize);
        }

        StateHasChanged();
    }

    /// <summary>
    /// Remove a prize from the current list.
    /// </summary>
    /// <param name="prizeId">Id of the prize to remove.</param>
    public void RemovePrizeFromList(int prizeId)
    {
        var existingPrizeIndex = _prizes.FindIndex(p => p.Id == prizeId);
        if (existingPrizeIndex != -1)
        {
            _prizes.RemoveAt(existingPrizeIndex);
            StateHasChanged();
        }
    }

    /// <summary>
    /// Load prizes for selected department when component initializes.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await RefreshPrizes();
        await base.OnInitializedAsync();
    }
}
