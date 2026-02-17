
namespace Organization.Blazor.Layout.User;

partial class DepartmentComponent
{
    private FormResultComponent _updateResult {get; set; } = null!;
    [Parameter] public TAppUserDepartment? AppUserDepartment { get; set; }
    [Inject] private IAccountService AccountService { get; set; } = default!;
    private async Task HandleUpdateRoleAsync()
    {
        _updateResult.ClearFormResult();
        if (AppUserDepartment != null)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            var result = await AccountService.AddUpdateAppUserDepartmentAsync(AppUserDepartment, cts.Token);
            if (result.Item1 != null)
            {
                AppUserDepartment = result.Item1;
                _updateResult.SetFormResult(new FormResult { Succeeded = true, ErrorList = ["Data updated successfully"] }, 2);
            } else if (result.Item2 != null)
            {
                _updateResult.SetFormResult(result.Item2, 2);
            }
        }
    }

    private async Task HandleDeleteAsync()
    {
        _updateResult.ClearFormResult();
        if (AppUserDepartment != null)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            var result = await AccountService.DeleteAppUserDepartmentAsync(AppUserDepartment, cts.Token);
            if (result.Succeeded)
            {
                AppUserDepartment = null;
            } else if (result != null)
            {
                _updateResult.SetFormResult(result, 2);
            }
        }
    }
}
