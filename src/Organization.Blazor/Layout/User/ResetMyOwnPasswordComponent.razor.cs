namespace Organization.Blazor.Layout.User;

partial class ResetMyOwnPasswordComponent : ComponentBase
{
    private bool isBusy;

    private FormResultComponent FormResult { get; set; } = null!;

    private RequestPasswordResetModel RequestModel { get; set; } = new();

    private SelfResetPasswordModel ResetModel { get; set; } = new();

    private bool HasResetToken => !string.IsNullOrWhiteSpace(UserId) && !string.IsNullOrWhiteSpace(ResetToken);

    private bool ShowInvalidLinkMessage =>
        (!string.IsNullOrWhiteSpace(UserId) || !string.IsNullOrWhiteSpace(ResetToken)) && !HasResetToken;

    private string InfoText => HasResetToken
        ? "Choose a new password for your account."
        : "Enter your email address and we will send you a password reset link if the account exists.";
    private string? CheckSpamText => HasResetToken
        ? null
        : "Remember to check your spam folder if you don't see the email in your inbox."
;

    [Parameter] public string? UserId { get; set; }

    [Parameter] public string? ResetToken { get; set; }

    [Inject] private IResetPasswordService ResetPasswordService { get; set; } = default!;

    protected override void OnParametersSet()
    {
        ResetModel.UserId = UserId;
        ResetModel.ResetToken = ResetToken;
    }

    private async Task HandleRequestPasswordResetAsync()
    {
        isBusy = true;
        FormResult.ClearFormResult();
        StateHasChanged();

        try
        {
            var result = await ResetPasswordService.RequestPasswordResetAsync(RequestModel);
            FormResult.SetFormResult(result);
            if (result.Succeeded)
            {
                RequestModel = new RequestPasswordResetModel();
            }
        }
        catch (Exception ex)
        {
            FormResult.SetFormResult(new FormResult { Succeeded = false, ErrorList = ["An error occurred while requesting a password reset. " + ex.Message] });
        }
        finally
        {
            isBusy = false;
            StateHasChanged();
        }
    }

    private async Task HandleResetOwnPasswordAsync()
    {
        isBusy = true;
        FormResult.ClearFormResult();
        StateHasChanged();

        try
        {
            var result = await ResetPasswordService.ResetOwnPasswordAsync(ResetModel);
            FormResult.SetFormResult(result, result.Succeeded ? 5 : 0);
            if (result.Succeeded)
            {
                ResetModel = new SelfResetPasswordModel
                {
                    UserId = UserId,
                    ResetToken = ResetToken
                };
            }
        }
        catch (Exception ex)
        {
            FormResult.SetFormResult(new FormResult { Succeeded = false, ErrorList = ["An error occurred while resetting your password. " + ex.Message] });
        }
        finally
        {
            isBusy = false;
            StateHasChanged();
        }
    }
}