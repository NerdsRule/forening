using Microsoft.JSInterop;

namespace Organization.Blazor.Layout.User;

public partial class WebAuthnComponent
{
    private FormResultComponent formResult = null!;
    private bool isBusy;
    private List<WebAuthnCredentialModel> PasskeyCredentials { get; set; } = [];
    private Dictionary<int, string> FriendlyNameEdits { get; set; } = [];
    private string NewPasskeyFriendlyName { get; set; } = string.Empty;

    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILocalStorageService LocalStorageService { get; set; } = default!;

    [Parameter] public bool AllowLogin { get; set; }
    [Parameter] public bool AllowRegistration { get; set; }
    [Parameter] public bool NavigateOnLoginSuccess { get; set; }
    private string PasskeyLoginEmail { get; set; } = string.Empty;

    /// <summary>
    /// On component initialization, attempt to load any stored email for passkey login from local storage and, if registration is allowed, load the list of registered passkey credentials for the authenticated user to display in the UI.
    /// </summary>
    /// <returns>The asynchronous task.</returns>
    protected override async Task OnInitializedAsync()
    {
        var storedEmail = await LocalStorageService.GetItemAsync<string>(StaticUserInfoBlazor.PasskeyLoginEmailStorageKey);
        if (!string.IsNullOrWhiteSpace(storedEmail))
        {
            PasskeyLoginEmail = storedEmail;
        }

        if (AllowRegistration)
        {
            await LoadPasskeysAsync();
        }
    }

    /// <summary>
    /// Handle input event for passkey login email to update the value and store it in local storage for future use.
    /// </summary>
    /// <param name="args">Change event arguments from the input field.</param>
    private async Task OnPasskeyEmailInput(ChangeEventArgs args)
    {
        PasskeyLoginEmail = args.Value?.ToString() ?? string.Empty;
        
        if (IsValidEmail(PasskeyLoginEmail))
        {
            await LocalStorageService.SetItemAsync(StaticUserInfoBlazor.PasskeyLoginEmailStorageKey, PasskeyLoginEmail);
        }
    }

    /// <summary>
    /// Simple email validation to check if the input is a valid email format before storing it in local storage.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email is valid; otherwise, false.</returns>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Handle the passkey registration process by interacting with the account service and invoking JavaScript functions for WebAuthn operations.
    /// </summary>
    /// <returns>The asynchronous task.</returns>
    private async Task HandlePasskeyRegistrationAsync()
    {
        await RunBusyAsync(async () =>
        {
            formResult.ClearFormResult();

            var (beginResult, error) = await AccountService.BeginPasskeyRegistrationAsync();
            if (beginResult is null)
            {
                formResult.SetFormResult(error ?? new FormResult { Succeeded = false, ErrorList = ["Failed to begin passkey registration."] });
                return;
            }

            var credentialJson = await JsRuntime.InvokeAsync<string>("organizationWebAuthn.createCredentialJson", beginResult.Options);
            var completeResult = await AccountService.CompletePasskeyRegistrationAsync(new WebAuthnCompleteRequest
            {
                RequestId = beginResult.RequestId,
                CredentialJson = credentialJson,
                FriendlyName = NewPasskeyFriendlyName
            });

            formResult.SetFormResult(completeResult, completeResult.Succeeded ? 3 : 0);
            if (completeResult.Succeeded)
            {
                NewPasskeyFriendlyName = string.Empty;
                await LoadPasskeysAsync();
            }
        });
    }

    ///<summary>
    /// Handle updating the friendly name of a registered passkey credential.
    ///</summary>
    /// <param name="credentialId">The ID of the credential to update.</param>
    /// <returns>The asynchronous task.</returns>
    private async Task HandleUpdateFriendlyNameAsync(int credentialId)
    {
        await RunBusyAsync(async () =>
        {
            formResult.ClearFormResult();

            if (!FriendlyNameEdits.TryGetValue(credentialId, out var friendlyName) || string.IsNullOrWhiteSpace(friendlyName))
            {
                formResult.SetFormResult(new FormResult { Succeeded = false, ErrorList = ["Friendly name is required."] });
                return;
            }

            var result = await AccountService.UpdatePasskeyFriendlyNameAsync(credentialId, friendlyName);
            formResult.SetFormResult(result, result.Succeeded ? 2 : 0);
            if (result.Succeeded)
            {
                await LoadPasskeysAsync();
            }
        });
    }

    /// <summary>
    /// Handle deleting a registered passkey credential by interacting with the account service and updating the UI accordingly.
    /// </summary>
    /// <param name="credentialId">The ID of the credential to delete.</param>
    /// <returns>The asynchronous task.</returns>
    private async Task HandleDeletePasskeyAsync(int credentialId)
    {
        await RunBusyAsync(async () =>
        {
            formResult.ClearFormResult();
            var result = await AccountService.DeletePasskeyCredentialAsync(credentialId);
            formResult.SetFormResult(result, result.Succeeded ? 2 : 0);
            if (result.Succeeded)
            {
                await LoadPasskeysAsync();
            }
        });
    }

    /// <summary>
    /// Load the list of registered passkey credentials for the authenticated user and update the UI state accordingly.
    /// </summary>
    private async Task LoadPasskeysAsync()
    {
        PasskeyCredentials = await AccountService.GetPasskeyCredentialsAsync();
        FriendlyNameEdits = PasskeyCredentials.ToDictionary(c => c.Id, c => c.FriendlyName ?? string.Empty);
        StateHasChanged();
    }

    /// <summary>
    /// Get the current friendly name edit value for a given credential ID, returning an empty string if no edit value is found.
    /// </summary>
    /// <param name="credentialId">The ID of the credential for which to get the friendly name edit value.</param>
    /// <returns>The current friendly name edit value or an empty string if not found.</returns>
    private string GetFriendlyNameEdit(int credentialId)
    {
        return FriendlyNameEdits.TryGetValue(credentialId, out var value) ? value : string.Empty;
    }

    /// <summary>
    /// Set the friendly name edit value for a given credential ID in the local state dictionary to track changes made by the user in the input fields.
    /// </summary>
    /// <param name="credentialId">The ID of the credential for which to set the friendly name edit value.</param>
    /// <param name="value">The new friendly name value to set for the specified credential ID.</param>
    private void SetFriendlyNameEdit(int credentialId, string? value)
    {
        FriendlyNameEdits[credentialId] = value ?? string.Empty;
    }

    /// <summary>
    /// Handle the passkey login process by interacting with the account service and invoking JavaScript functions for WebAuthn operations, while managing busy state and form results for user feedback.
    /// </summary>
    /// <returns>The asynchronous task.</returns>
    private async Task HandlePasskeyLoginAsync()
    {
        await RunBusyAsync(async () =>
        {
            formResult.ClearFormResult();

            var emailToUse = PasskeyLoginEmail.Trim();

            if (string.IsNullOrWhiteSpace(emailToUse))
            {
                formResult.SetFormResult(new FormResult { Succeeded = false, ErrorList = ["Email is required for passkey login."] });
                return;
            }

            var (beginResult, error) = await AccountService.BeginPasskeyLoginAsync(emailToUse);
            if (beginResult is null)
            {
                formResult.SetFormResult(error ?? new FormResult { Succeeded = false, ErrorList = ["Failed to begin passkey login."] });
                return;
            }

            var credentialJson = await JsRuntime.InvokeAsync<string>("organizationWebAuthn.getAssertionJson", beginResult.Options);
            var completeResult = await AccountService.CompletePasskeyLoginAsync(new WebAuthnCompleteRequest
            {
                RequestId = beginResult.RequestId,
                CredentialJson = credentialJson
            });

            formResult.SetFormResult(completeResult, completeResult.Succeeded ? 2 : 0);
            if (completeResult.Succeeded && NavigateOnLoginSuccess)
            {
                Navigation.NavigateTo("/", forceLoad: true);
            }
        });
    }

    /// <summary>
    /// Utility method to run an asynchronous action while managing a busy state flag and handling exceptions to provide user feedback through form results, ensuring that the UI reflects the busy state appropriately during the operation.   
    /// </summary>
    /// <param name="action">The asynchronous action to execute while managing busy state and form results.</param>
    /// <returns>The asynchronous task.</returns>
    private async Task RunBusyAsync(Func<Task> action)
    {
        isBusy = true;
        StateHasChanged();
        try
        {
            await action();
        }
        catch (JSException jsException)
        {
            formResult.SetFormResult(new FormResult { Succeeded = false, ErrorList = [jsException.Message] });
        }
        catch (Exception ex)
        {
            formResult.SetFormResult(new FormResult { Succeeded = false, ErrorList = [ex.Message] });
        }
        finally
        {
            isBusy = false;
            StateHasChanged();
        }
    }
}
