
namespace Organization.Blazor.Layout.User;

partial class OrganizationComponent
{
    private FormResult? _updateResult;
    private Dictionary<int, DepartmentComponent> _userDepartmentsDict = new();
    private List<TDepartment> _departments = new();
    private TAppUserDepartment _newAppUserDepartment = new();
    private async Task HandleUpdateRoleAsync()
    {
        _updateResult = null;
        if (AppUserOrganization != null)
        {
            var result = await AccountService.AddUpdateAppUserOrganizationAsync(AppUserOrganization);
            if (result.Item1 != null)
            {
                AppUserOrganization = result.Item1;
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
        if (AppUserOrganization != null)
        {
            var result = await AccountService.DeleteAppUserOrganizationAsync(AppUserOrganization);
            if (result.Succeeded)
            {
                AppUserOrganization = null;
            } else
            {
                _updateResult = result;
            }
        }
    }

    private async Task HandleAddDepartmentAsync()
    {
        var result = await AccountService.AddUpdateAppUserDepartmentAsync(_newAppUserDepartment);
        if (result.appUserDepartment != null)
        {
            AppUserDepartments.Add(result.appUserDepartment);
            _newAppUserDepartment = new TAppUserDepartment { AppUserId = StaticUserInfoBlazor.User!.Id };
        } else
        {
            _updateResult = result.formResult;
        }
    }
    protected override async Task OnInitializedAsync()
    {
        if (AppUserOrganization != null)
        {
            var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
            _departments = (await AccountService.GetDepartmentsByOrganizationIdAsync(AppUserOrganization.OrganizationId, ct)).departments ?? new List<TDepartment>();
            _newAppUserDepartment.AppUserId = AppUserOrganization.AppUserId;
        }
        await base.OnInitializedAsync();
    }
    [Parameter] public TAppUserOrganization? AppUserOrganization { get; set; }
    [Parameter] public List<TAppUserDepartment> AppUserDepartments { get; set; } = new();
    [Inject] private IAccountService AccountService { get; set; } = default!;
}
