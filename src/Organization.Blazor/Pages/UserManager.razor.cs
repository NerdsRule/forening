
namespace Organization.Blazor.Pages;

partial class UserManager
{
    private List<UserModel> _users = new();
    private string selectedUserId = string.Empty;

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
