

namespace Organization.Blazor.Layout.User;

partial class UserComponent
{
    private FormResult? _formResult;

    /// <summary>
    /// Handle submit user
    /// </summary>
    /// <returns></returns>
    private async Task HandleSubmitUser()
    {
        _formResult = null;
        if (User != null)
        {
            _formResult = await AccountService.UpdateUserAsync(User);
        }
    }
    [Parameter] public UserModel? User { get; set; }
    [Inject] private IAccountService AccountService { get; set; } = default!;
}
