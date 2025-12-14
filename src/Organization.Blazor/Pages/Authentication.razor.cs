
using System.ComponentModel.DataAnnotations;
using Organization.Shared.Interfaces;
using Organization.Shared.Identity;

namespace Organization.Blazor.Pages;

partial class Authentication : ComponentBase
{
[Parameter] public string? Action { get; set; }
    
    private LoginModel loginModel = new();
    private string errorMessage = string.Empty;
    private bool isLoading = false;
    
    protected override async Task OnInitializedAsync()
    {
        if (Action?.ToLowerInvariant() == "logout")
        {
            await HandleLogout();
        }
    }
    
    private async Task HandleLogin()
    {
        isLoading = true;
        errorMessage = string.Empty;
        StateHasChanged();
        
        try
        {
            loginModel.RememberMe = true;
            var result = await AccountService.LoginAsync(loginModel);
            
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
            errorMessage = "An error occurred during login.";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
    
    private async Task HandleLogout()
    {
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

    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
}
