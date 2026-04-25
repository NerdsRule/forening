
namespace Organization.Blazor.Pages;

partial class UserManager
{
    private List<UserModel> _users = new();
    private string selectedUserId = string.Empty;
    private string _userSearchText = string.Empty;
    private UserModel? selectedUser {get; set;}

    private static string GetUserDisplayText(UserModel user)
        => $"{user.DisplayName} ({user.Email})";

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

    private async Task SelectedUserChangedAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            selectedUserId = string.Empty;
            _userSearchText = string.Empty;
            selectedUser = null;
            return;
        }
        selectedUserId = userId;
        var selected = _users.FirstOrDefault(user => user.Id == userId);
        if (selected is not null)
            _userSearchText = GetUserDisplayText(selected);
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        selectedUser = await  AccountService.GetUserByIdAsync(userId, ct);
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
    [Inject] private NavigationManager Navigation { get; set; } = default!;
}
