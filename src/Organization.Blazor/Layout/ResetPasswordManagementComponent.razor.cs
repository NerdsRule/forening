namespace Organization.Blazor.Layout;

public partial class ResetPasswordManagementComponent : ComponentBase
{
    private readonly List<TResetPassword> _resetRequests = [];
    private FormResultComponent _formResult { get; set; } = null!;
    private bool _isBusy;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _formResult?.ClearFormResult();
        _isBusy = true;

        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = tokenSource.Token;
            var (rows, result) = await ResetPasswordService.GetResetRequestsAsync(StaticUserInfoBlazor.SelectedOrganization!.OrganizationId, cancellationToken);
            _resetRequests.Clear();
            if (rows is not null)
            {
                _resetRequests.AddRange(rows);
            }
            else
            {
                _formResult?.SetFormResult(result ?? new FormResult { Succeeded = false, ErrorList = ["Unable to load reset requests."] }, 0);
            }
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task DeleteAsync(int id)
    {
        _formResult.ClearFormResult();
        _isBusy = true;

        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = tokenSource.Token;
            var result = await ResetPasswordService.DeleteResetRequestAsync(StaticUserInfoBlazor.SelectedOrganization!.OrganizationId, id, cancellationToken);
            _formResult.SetFormResult(result, result.Succeeded ? 5 : 0);

            if (result.Succeeded)
            {
                var existing = _resetRequests.FirstOrDefault(r => r.Id == id);
                if (existing is not null)
                {
                    _resetRequests.Remove(existing);
                }
            }
        }
        finally
        {
            _isBusy = false;
        }
    }

    [Inject] private IResetPasswordService ResetPasswordService { get; set; } = default!;
}