
namespace Organization.Blazor.Layout.User;

partial class OrganizationComponent
{
    private FormResultComponent _updateResult { get; set; } = null!;
    private Dictionary<int, DepartmentComponent> _userDepartmentsDict = new();
    private List<TDepartment> _departments = new();
    private TAppUserDepartment _newAppUserDepartment = new();
    [Parameter] public TAppUserOrganization? AppUserOrganization { get; set; }
    [Parameter] public List<TAppUserDepartment> AppUserDepartments { get; set; } = new();
    [Inject] private IAccountService AccountService { get; set; } = default!;

    private async Task HandleUpdateRoleAsync()
    {
        _updateResult.ClearFormResult();
        if (AppUserOrganization != null)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            var result = await AccountService.AddUpdateAppUserOrganizationAsync(AppUserOrganization, cts.Token);
            if (result.Item1 != null)
            {
                AppUserOrganization = result.Item1;
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
        if (AppUserOrganization != null)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            var result = await AccountService.DeleteAppUserOrganizationAsync(AppUserOrganization, cts.Token);
            if (result.Succeeded)
            {
                AppUserOrganization = null;
            } else if (result != null)
            {
                _updateResult.SetFormResult(result, 2);
            }
        }
    }

    private async Task HandleAddDepartmentAsync()
    {
        _updateResult.ClearFormResult();
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
        var result = await AccountService.AddUpdateAppUserDepartmentAsync(_newAppUserDepartment, cts.Token);
        if (result.appUserDepartment != null)
        {
            result.appUserDepartment.Department = _departments.FirstOrDefault(d => d.Id == result.appUserDepartment.DepartmentId);
            AppUserDepartments.Add(result.appUserDepartment);
            _newAppUserDepartment = new TAppUserDepartment { AppUserId = StaticUserInfoBlazor.User!.Id };
        } else if (result.formResult != null)
        {
            _updateResult.SetFormResult(result.formResult, 2);
        }
    }
    protected override async Task OnInitializedAsync()
    {
        if (AppUserOrganization != null)
        {
            var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
            var departments = await AccountService.GetDepartmentsByOrganizationIdAsync(AppUserOrganization.OrganizationId, AppUserOrganization.AppUserId, ct);
            if (departments.departments != null)            
            {
                _departments = departments.departments;
            }
            if (departments.formResult != null)
            {
                _updateResult.SetFormResult(departments.formResult, 2);
            }
            _newAppUserDepartment.AppUserId = AppUserOrganization.AppUserId;
        }
        await base.OnInitializedAsync();
    }
}
    
