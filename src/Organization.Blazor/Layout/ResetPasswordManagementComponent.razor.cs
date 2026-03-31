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
            var rows = await ResetPasswordService.GetResetRequestsAsync();
            _resetRequests.Clear();
            if (rows is not null)
            {
                _resetRequests.AddRange(rows);
            }
            else
            {
                _formResult?.SetFormResult(new FormResult { Succeeded = false, ErrorList = ["Unable to load reset requests."] });
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
            var result = await ResetPasswordService.DeleteResetRequestAsync(id);
            _formResult.SetFormResult(result);

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