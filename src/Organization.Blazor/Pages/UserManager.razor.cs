
namespace Organization.Blazor.Pages;

partial class UserManager
{
    private List<UserModel> _users = new();
    private string selectedUserId = string.Empty;
    private UserModel? selectedUser {get; set;}

    private async Task SelectedUserChangedAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            selectedUserId = string.Empty;
            selectedUser = null;
            return;
        }
        selectedUserId = userId;
        selectedUser = await  AccountService.GetUserByIdAsync(userId);
    }

    protected override async Task OnInitializedAsync()
    {
        if (StaticUserInfoBlazor.User is null)
        {
            Navigation.NavigateTo("/");
            return;
        }
        _users = await AccountService.GetUsersAsync();
        await base.OnInitializedAsync();
    }

    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
}
