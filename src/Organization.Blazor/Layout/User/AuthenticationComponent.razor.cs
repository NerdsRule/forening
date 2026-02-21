
namespace Organization.Blazor.Layout.User;

partial class AuthenticationComponent : ComponentBase
{

    FormResultComponent FormResult { get; set; } = null!;
    private LoginModel loginModel = new();
    private bool isLoading = false;
    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Parameter] public string? Action { get; set; }

    /// <summary>
    /// When the component is initialized, it checks if the Action parameter is set to "logout". If so, it calls the HandleLogout method to log the user out. This allows the component to automatically handle logout actions when navigated to with the appropriate query parameter, ensuring a seamless user experience when logging out of the application.
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        if (Action?.ToLowerInvariant() == "logout")
        {
            await HandleLogout();
        }
    }
    
    /// <summary>
    /// This method is responsible for handling the login process when the user submits their credentials. It sets a loading state, clears any previous error messages, and then attempts to log in using the AccountService. If the login is successful, it navigates the user to the home page. If there are errors during login, it captures and displays them. Finally, it resets the loading state regardless of the outcome, ensuring that the UI reflects the current state of the login process accurately.
    /// </summary>
    /// <returns></returns>
    private async Task HandleLogin()
    {
        isLoading = true;
        FormResult.ClearFormResult();
        StateHasChanged();
        
        try
        {
            loginModel.RememberMe = true;
            var result = await AccountService.LoginAsync(loginModel);
            
            if (result.Succeeded)
            {
                FormResult.SetFormResult(result, timeoutSeconds: 2);
            }
            else
            {
                FormResult.SetFormResult(result);
            }
        }
        catch (Exception ex)
        {
            FormResult.SetFormResult(new FormResult {  Succeeded = false, ErrorList = ["An error occurred during login. " + ex.Message ]});
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
    
    /// <summary>
    /// This method handles the logout process by calling the LogoutAsync method of the AccountService. After attempting to log out, it navigates the user back to the home page regardless of whether the logout was successful or if an exception occurred. This ensures that the user is redirected appropriately after initiating a logout action, maintaining a consistent user experience even in cases where there might be issues during the logout process.
    /// </summary>
    /// <returns></returns>
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


}
