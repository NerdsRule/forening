
namespace Organization.Blazor.Layout.User;

partial class DepartmentComponent
{
    private FormResultComponent _updateResult {get; set; } = null!;
    private HashSet<Shared.RolesEnum> _selectedRoles = [];

    [Parameter] public TAppUserDepartment? AppUserDepartment { get; set; }
    [Inject] private IAccountService AccountService { get; set; } = default!;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _selectedRoles = AppUserDepartment?.Roles.Select(r => r.Role).ToHashSet() ?? [];
    }

    private bool IsRoleSelected(Shared.RolesEnum role) => _selectedRoles.Contains(role);

    private void OnRoleChanged(Shared.RolesEnum role, object? value)
    {
        var isChecked = value as bool? == true;
        if (isChecked)
        {
            _selectedRoles.Add(role);
        }
        else
        {
            _selectedRoles.Remove(role);
        }
    }

    private async Task HandleUpdateRoleAsync()
    {
        _updateResult.ClearFormResult();
        if (AppUserDepartment != null)
        {
            AppUserDepartment.Roles = _selectedRoles
                .Select(role => new TAppUserDepartmentRole { Role = role })
                .ToList();

            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            var result = await AccountService.AddUpdateAppUserDepartmentAsync(AppUserDepartment, cts.Token);
            if (result.Item1 != null)
            {
                AppUserDepartment = result.Item1;
                _selectedRoles = AppUserDepartment.Roles.Select(r => r.Role).ToHashSet();
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
