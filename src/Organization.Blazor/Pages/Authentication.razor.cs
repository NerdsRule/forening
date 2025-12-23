

namespace Organization.Blazor.Pages;

partial class Authentication : ComponentBase
{
[Parameter] public string? Action { get; set; }
    
    private string errorMessage = string.Empty;
    private ChangePasswordModel changePasswordModel = new ChangePasswordModel{ CurrentPassword = string.Empty, NewPassword = string.Empty };
    private bool isLoading = false;
    
    protected override async Task OnInitializedAsync()
    {
        if (Action?.ToLowerInvariant() == "logout")
        {
            await HandleLogout();
        }
    }
    
    private async Task HandleLogout()
    {
        Console.WriteLine("Authentication: Handling logout...");
        try
        {
            await AccountService.LogoutAsync();
            Navigation.NavigateTo("/", forceLoad: true);
        }
        catch (Exception)
        {
            Navigation.NavigateTo("/", forceLoad: true);
        }
    }

    private async Task HandleChangePassword()
    {
        isLoading = true;
        errorMessage = string.Empty;
        StateHasChanged();
        try
        {
            var result = await AccountService.ChangePasswordAsync(changePasswordModel);
            if (result.Succeeded)
            {
                Navigation.NavigateTo("/", forceLoad: true);
            }
            else
            {
                errorMessage = string.Join(", ", result.ErrorList);
            }
        }
        catch (Exception ex)
        {
            errorMessage = "An error occurred during password change. " + ex.Message;
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
}
