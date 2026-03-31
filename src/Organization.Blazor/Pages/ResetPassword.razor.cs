
namespace Organization.Blazor.Pages;

partial class ResetPassword
{
    private List<UserModel> _users = new();
    private ResetPasswordModel _resetPasswordModel { get; set; } = new();
    private FormResultComponent FormResult { get; set;} = null!;
    private UserModel? _selectedUser {get; set;}
    private string? _selectedUserId { get; set; }

    private async Task SelectedUserChangedAsync(string? userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            _selectedUserId = null;
            _selectedUser = null;
            _resetPasswordModel.UserId = null;
            return;
        }
        _selectedUser = _users.FirstOrDefault(u => u.Id == userId);
        _resetPasswordModel.UserId = userId;
        _selectedUserId = userId;
    }

    private async Task HandleValidSubmit()
    {
        FormResult.ClearFormResult();
        var res = await ResetPasswordService.ResetPasswordAsync(_resetPasswordModel);
        FormResult.SetFormResult(res);
    }

    protected override async Task OnInitializedAsync()
    {
        if (StaticUserInfoBlazor.User is null)
        {
            Navigation.NavigateTo("/");
            return;
        }
        _users = await AccountService.GetUsersAsync(StaticUserInfoBlazor.SelectedOrganization?.OrganizationId ?? 0, StaticUserInfoBlazor.SelectedDepartment?.DepartmentId ?? 0);
        await base.OnInitializedAsync();
    }

    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private IResetPasswordService ResetPasswordService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
}
