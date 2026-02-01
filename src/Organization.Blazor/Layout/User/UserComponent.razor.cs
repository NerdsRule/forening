

namespace Organization.Blazor.Layout.User;

partial class UserComponent
{
    private FormResult? _formResult;
    private Dictionary<int, OrganizationComponent> _userOrganizationsDict = new();
    

    /// <summary>
    /// Handle submit user
    /// </summary>
    /// <returns></returns>
    private async Task HandleSubmitUser()
    {
        _formResult = null;
        if (User != null)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            _formResult = await AccountService.UpdateUserAsync(User, cts.Token);
        }
    }

    /// <summary>
    /// Delete user
    /// </summary>
    /// <returns></returns>
    private async Task HandleDeleteUserAsync()
    {
        _formResult = null;
        if (User != null)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            _formResult = await AccountService.DeleteUserAsync(User.Id, cts.Token);
        }
    }
    [Parameter] public UserModel? User { get; set; }
    [Inject] private IAccountService AccountService { get; set; } = default!;
}
