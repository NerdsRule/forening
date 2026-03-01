namespace Organization.Blazor.Layout.User;

partial class WebAuthComponent : ComponentBase
{
    [Parameter] public bool ShowLogin { get; set; } = false;
    [Parameter] public bool ShowRegister { get; set; } = false;
    [Parameter] public string? Email { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    FormResultComponent ResultComponent { get; set; } = default!;
    FormResult FormResultData { get; set; } = new FormResult();
    private bool IsBusy { get; set; }
    private bool IsError { get; set; }
    private string? PasskeyFriendlyName { get; set; }

    
    /// <summary>
    /// Handles the passkey registration process. Validates WebAuthn support,
    /// initializes registration options, creates a credential, and completes
    /// the registration on the server. Displays appropriate success or error messages based on the outcome.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandlePasskeyRegistrationAsync()
    {
        await ExecuteAsync(async () =>
        {
            var supported = await JsRuntime.InvokeAsync<bool>("orgWebAuth.hasWebAuthn");
            if (!supported)
            {
                FormResultData.Succeeded = false;
                FormResultData.ErrorList = [ "WebAuthn is not supported in this browser." ];
                ResultComponent.SetFormResult(FormResultData);
                return;
            }

            var options = await AccountService.BeginWebAuthRegistrationAsync();
            if (options is null)
            {
                FormResultData.Succeeded = false;
                FormResultData.ErrorList = [ "Unable to initialize passkey registration." ];
                ResultComponent.SetFormResult(FormResultData);
                return;
            }

            var clientResult = await JsRuntime.InvokeAsync<WebAuthRegisterClientResult>("orgWebAuth.createCredential", options);
            if (clientResult is null || string.IsNullOrWhiteSpace(clientResult.CredentialId))
            {
                FormResultData.Succeeded = false;
                FormResultData.ErrorList = [ "Passkey registration was cancelled or failed." ];
                ResultComponent.SetFormResult(FormResultData);
                return;
            }

            if (string.IsNullOrWhiteSpace(clientResult.PublicKeySpki))
            {
                FormResultData.Succeeded = false;
                FormResultData.ErrorList = [ "This browser did not return a usable public key for registration." ];
                ResultComponent.SetFormResult(FormResultData);
                return;
            }

            var result = await AccountService.CompleteWebAuthRegistrationAsync(new WebAuthRegisterCompleteRequest
            {
                CredentialId = clientResult.CredentialId,
                ClientDataJson = clientResult.ClientDataJson,
                PublicKeySpki = clientResult.PublicKeySpki,
                PublicKeyAlgorithm = clientResult.PublicKeyAlgorithm,
                FriendlyName = PasskeyFriendlyName
            });

            if (result.Succeeded)
            {
                PasskeyFriendlyName = null;
                FormResultData.Succeeded = true;
                FormResultData.ErrorList = [ "Passkey registered." ];
                ResultComponent.SetFormResult(FormResultData, 2);
                return;
            }

            ResultComponent.SetFormResult(result);
        });
    }

    /// <summary>
    /// Handles the passkey authentication process. Validates input, checks WebAuthn support,
    /// retrieves authentication options, gets an assertion from the client, and completes authentication on the server. Displays appropriate success or error messages based on the outcome.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandlePasskeyLoginAsync()
    {
        await ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                FormResultData.Succeeded = false;
                FormResultData.ErrorList = [ "Enter your email before using passkey login." ];
                ResultComponent.SetFormResult(FormResultData);
                return;
            }

            var supported = await JsRuntime.InvokeAsync<bool>("orgWebAuth.hasWebAuthn");
            if (!supported)
            {
                FormResultData.Succeeded = false;
                FormResultData.ErrorList = [ "WebAuthn is not supported in this browser." ];
                ResultComponent.SetFormResult(FormResultData);
                return;
            }

            var options = await AccountService.BeginWebAuthAuthenticationAsync(Email);
            if (options is null)
            {
                FormResultData.Succeeded = false;
                FormResultData.ErrorList = [ "No passkey options found for this email." ];
                ResultComponent.SetFormResult(FormResultData);
                return;
            }

            var assertion = await JsRuntime.InvokeAsync<WebAuthAuthenticateClientResult>("orgWebAuth.getAssertion", options);
            if (assertion is null || string.IsNullOrWhiteSpace(assertion.CredentialId))
            {
                FormResultData.Succeeded = false;
                FormResultData.ErrorList = [ "Passkey authentication was cancelled or failed." ];
                ResultComponent.SetFormResult(FormResultData);
                return;
            }

            var result = await AccountService.CompleteWebAuthAuthenticationAsync(new WebAuthAuthenticateCompleteRequest
            {
                Email = Email,
                CredentialId = assertion.CredentialId,
                ClientDataJson = assertion.ClientDataJson,
                AuthenticatorData = assertion.AuthenticatorData,
                Signature = assertion.Signature,
                UserHandle = assertion.UserHandle
            });

            if (result.Succeeded)
            {
                ResultComponent.SetFormResult(result, 2);
                Navigation.NavigateTo("/", forceLoad: true);
                return;
            }
            
            ResultComponent.SetFormResult(result);
        });
    }

    /// <summary>
    /// Executes an asynchronous action with standardized error handling and UI state management. Sets the component to a busy state, clears previous results, and executes the provided action. Catches and displays any exceptions that occur during execution, and resets the busy state afterward.
    /// </summary>
    /// <param name="action">The asynchronous action to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecuteAsync(Func<Task> action)
    {
        try
        {
            IsBusy = true;
            ResultComponent.ClearFormResult();
            IsError = false;
            await action();
        }
        catch (JSException ex)
        {
            FormResultData.Succeeded = false;
            FormResultData.ErrorList = [ $"JavaScript error: {ex.Message}" ];
            ResultComponent.SetFormResult(FormResultData);
        }
        catch (Exception ex)
        {
            FormResultData.Succeeded = false;
            FormResultData.ErrorList = [ $"Error: {ex.Message}" ];
            ResultComponent.SetFormResult(FormResultData);
        }
        finally
        {
            IsBusy = false;
            StateHasChanged();
        }
    }
}
