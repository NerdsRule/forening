
namespace Organization.Blazor.Pages;

partial class ResetPassword
{
    private List<UserModel> _users = new();
    private ResetPasswordModel _resetPasswordModel { get; set; } = new();
    private FormResultComponent FormResult { get; set;} = null!;
    private UserModel? _selectedUser {get; set;}
    private string _userSearchText = string.Empty;
    private string? _selectedUserId { get; set; }

    private static string GetUserDisplayText(UserModel user)
        => $"{user.DisplayName} ({user.UserName})";

    private Task OnUserSearchChanged(string value)
    {
        _userSearchText = value;
        return Task.CompletedTask;
    }

    private async Task OnUserSelectedFromTypeAhead(string selectedText)
    {
        var selected = _users.FirstOrDefault(user =>
            string.Equals(GetUserDisplayText(user), selectedText, StringComparison.OrdinalIgnoreCase));

        if (selected is null)
            return;

        await SelectedUserChangedAsync(selected.Id);
    }

    private async Task SelectedUserChangedAsync(string? userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            _selectedUserId = null;
            _userSearchText = string.Empty;
            _selectedUser = null;
            _resetPasswordModel.UserId = null;
            return;
        }
        _selectedUser = _users.FirstOrDefault(u => u.Id == userId);
        _resetPasswordModel.UserId = userId;
        _selectedUserId = userId;
        if (_selectedUser is not null)
            _userSearchText = GetUserDisplayText(_selectedUser);
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
