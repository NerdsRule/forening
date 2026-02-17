
namespace Organization.Blazor.Pages;

partial class RegisterUser
{
    private RegisterModel _registerModel = new();
    private FormResultComponent FormResult { get; set; } = null!;

    private async Task HandleValidSubmit()
    {
        FormResult.ClearFormResult();
        // Handle the valid form submission, e.g., send data to the server
        var result = await AccountService.RegisterAsync(_registerModel);
        FormResult.SetFormResult(result);
    }

    protected override void OnInitialized()
    {
        // Initialization logic if needed
        _registerModel.OrganizationId = StaticUserInfoBlazor.SelectedOrganization?.Id ?? 0;
    }

    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IAccountService AccountService { get; set; } = null!;
}
