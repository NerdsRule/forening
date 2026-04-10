namespace Organization.Blazor.Layout.User;

partial class EmailConfirmationRequestComponent : ComponentBase
{
    private bool isBusy;

    private FormResultComponent FormResult { get; set; } = null!;

    [Inject] private IEmailConfirmationService EmailConfirmationService { get; set; } = default!;

    private async Task HandleRequestEmailConfirmationAsync()
    {
        isBusy = true;
        FormResult.ClearFormResult();
        StateHasChanged();

        try
        {
            var result = await EmailConfirmationService.RequestEmailConfirmationTokenAsync();
            FormResult.SetFormResult(result);
        }
        catch (Exception ex)
        {
            FormResult.SetFormResult(new FormResult
            {
                Succeeded = false,
                ErrorList = ["An error occurred while requesting the email confirmation link. " + ex.Message]
            });
        }
        finally
        {
            isBusy = false;
            StateHasChanged();
        }
    }
}