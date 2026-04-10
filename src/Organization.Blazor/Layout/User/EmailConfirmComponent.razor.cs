namespace Organization.Blazor.Layout.User;

partial class EmailConfirmComponent : ComponentBase
{
    private bool isBusy;
    private string? _lastAutoConfirmedToken;

    private FormResultComponent FormResult { get; set; } = null!;

    private FormResult? LastResult { get; set; }

    private bool HasToken => !string.IsNullOrWhiteSpace(Token);

    private bool ShowInvalidOrExpiredTokenMessage =>
        LastResult is { Succeeded: false } &&
        LastResult.ErrorList.Any(IsInvalidOrExpiredTokenMessage);

    private bool IsAdminUser =>
        StaticUserInfoBlazor.DepartmentRole == Shared.RolesEnum.DepartmentAdmin ||
        StaticUserInfoBlazor.OrganizationRole == Shared.RolesEnum.OrganizationAdmin ||
        StaticUserInfoBlazor.OrganizationRole == Shared.RolesEnum.EnterpriseAdmin;

    [Parameter] public string? Token { get; set; }

    [Inject] private IEmailConfirmationService EmailConfirmationService { get; set; } = default!;

    [Inject] private IAccountService AccountService { get; set; } = default!;

    [Inject] private IUiStateService UiStateService { get; set; } = default!;

    protected override async Task OnParametersSetAsync()
    {
        if (!HasToken || string.Equals(Token, _lastAutoConfirmedToken, StringComparison.Ordinal))
        {
            return;
        }

        _lastAutoConfirmedToken = Token;
        await ConfirmEmailInternalAsync();
    }

    private Task HandleConfirmEmailAsync()
    {
        return ConfirmEmailInternalAsync();
    }

    private async Task ConfirmEmailInternalAsync()
    {
        if (!HasToken)
        {
            LastResult = new FormResult
            {
                Succeeded = false,
                ErrorList = ["Token is required."]
            };

            FormResult?.SetFormResult(LastResult);
            return;
        }

        isBusy = true;
        FormResult?.ClearFormResult();
        StateHasChanged();

        try
        {
            var result = await EmailConfirmationService.ConfirmEmailAsync(new EmailConfirmationConfirmModel
            {
                Token = Token
            });

            LastResult = result;
            FormResult?.SetFormResult(result);

            if (result.Succeeded)
            {
                if (StaticUserInfoBlazor.User is not null)
                {
                    StaticUserInfoBlazor.User.EmailConfirmed = true;
                }

                _ = await AccountService.CheckAuthenticatedAsync();
                UiStateService.NotifyUserUpdated();
            }
        }
        catch (Exception ex)
        {
            LastResult = new FormResult
            {
                Succeeded = false,
                ErrorList = ["An error occurred while confirming your email. " + ex.Message]
            };

            FormResult?.SetFormResult(LastResult);
        }
        finally
        {
            isBusy = false;
            StateHasChanged();
        }
    }

    private static bool IsInvalidOrExpiredTokenMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return message.Contains("expired", StringComparison.OrdinalIgnoreCase)
            || message.Contains("lifetime", StringComparison.OrdinalIgnoreCase)
            || message.Contains("invalid", StringComparison.OrdinalIgnoreCase)
            || message.Contains("does not match", StringComparison.OrdinalIgnoreCase)
            || message.Contains("purpose", StringComparison.OrdinalIgnoreCase);
    }
}