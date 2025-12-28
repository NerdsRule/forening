

namespace Organization.Blazor.Layout.User;

partial class UserComponent
{
    private async Task HandleSubmitUser()
    {
        if (User != null)
        {
            //await AccountService.UpdateUserAsync(User);
        }
    }
    [Parameter] public UserModel? User { get; set; }
    [Inject] private IAccountService AccountService { get; set; } = default!;
}
