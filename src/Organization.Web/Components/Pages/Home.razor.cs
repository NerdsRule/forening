

namespace Organization.Web.Components.Pages;

partial class Home : ComponentBase
{
    

    private ClaimsPrincipal? _user { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await InitializeUserAsync();
    }

    private async Task InitializeUserAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        _user = authState.User;
    }

    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
}
