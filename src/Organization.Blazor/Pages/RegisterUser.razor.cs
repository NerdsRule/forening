
namespace Organization.Blazor.Pages;

partial class RegisterUser
{
    private RegisterModel _registerModel = new();
    private FormResult? _formResult;

    private async Task HandleValidSubmit()
    {
        // Handle the valid form submission, e.g., send data to the server
        _formResult = await AccountService.RegisterAsync(_registerModel);
    }

    protected override void OnInitialized()
    {
        // Initialization logic if needed
        _registerModel.OrganizationId = StaticUserInfoBlazor.SelectedOrganization?.Id ?? 0;
    }

    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IAccountService AccountService { get; set; } = null!;
}
