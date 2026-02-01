
namespace Organization.Blazor.Layout.User;

partial class DepartmentComponent
{
    private FormResult? _updateResult;
    private async Task HandleUpdateRoleAsync()
    {
        if (AppUserDepartment != null)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            var result = await AccountService.AddUpdateAppUserDepartmentAsync(AppUserDepartment, cts.Token);
            if (result.Item1 != null)
            {
                AppUserDepartment = result.Item1;
                _updateResult = new FormResult { Succeeded = true, ErrorList = ["Data updated successfully"] };
            } else
            {
                _updateResult = result.Item2;
            }
        }
    }

    private async Task HandleDeleteAsync()
    {
        _updateResult = null;
        if (AppUserDepartment != null)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            var result = await AccountService.DeleteAppUserDepartmentAsync(AppUserDepartment, cts.Token);
            if (result.Succeeded)
            {
                AppUserDepartment = null;
            } else
            {
                _updateResult = result;
            }
        }
    }
    [Parameter] public TAppUserDepartment? AppUserDepartment { get; set; }
    [Inject] private IAccountService AccountService { get; set; } = default!;
}
