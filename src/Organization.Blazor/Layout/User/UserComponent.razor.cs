

namespace Organization.Blazor.Layout.User;

partial class UserComponent
{
    private FormResultComponent _formResult { get; set; } = null!;
    private Dictionary<int, OrganizationComponent> _userOrganizationsDict = new();
    

    /// <summary>
    /// Handle submit user
    /// </summary>
    /// <returns></returns>
    private async Task HandleSubmitUser()
    {
        _formResult.ClearFormResult();
        if (User != null)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            var result = await AccountService.UpdateUserAsync(User, cts.Token);
            if (result != null)
            {
                _formResult.SetFormResult(result, 2);
            }
        }
    }

    /// <summary>
    /// Delete user
    /// </summary>
    /// <returns></returns>
    private async Task HandleDeleteUserAsync()
    {
        _formResult.ClearFormResult();
        if (User != null)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            var result = await AccountService.DeleteUserAsync(User.Id, cts.Token);
            if (result != null)
            {
                _formResult.SetFormResult(result, 2);
            }
        }
    }
    [Parameter] public UserModel? User { get; set; }
    [Inject] private IAccountService AccountService { get; set; } = default!;
}
