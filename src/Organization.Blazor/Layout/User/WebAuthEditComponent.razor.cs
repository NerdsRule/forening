namespace Organization.Blazor.Layout.User;

partial class WebAuthEditComponent : ComponentBase
{
    [Inject] private IAccountService AccountService { get; set; } = default!;

    private FormResultComponent ResultComponent { get; set; } = default!;
    private FormResult FormResultData { get; set; } = new FormResult();

    private List<WebAuthCredentialModel> Credentials { get; set; } = [];
    private bool IsBusy { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadCredentialsAsync();
    }

    private async Task LoadCredentialsAsync()
    {
        await ExecuteAsync(async () =>
        {
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            Credentials = await AccountService.GetWebAuthCredentialsAsync(cts.Token);
            FormResultData.Succeeded = true;
            FormResultData.ErrorList = [ "Passkey list updated." ];
            ResultComponent.SetFormResult(FormResultData, 2);
        });
    }

    private async Task RenameAsync(WebAuthCredentialModel item)
    {
        await ExecuteAsync(async () =>
        {
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            var result = await AccountService.RenameWebAuthCredentialAsync(item.Id, item.FriendlyName, cts.Token);
            if (!result.Succeeded)
            {
                FormResultData.Succeeded = false;
                FormResultData.ErrorList = [ result.ErrorList?.FirstOrDefault() ?? "Unable to rename passkey." ];
                ResultComponent.SetFormResult(FormResultData);
                return;
            }

            FormResultData.Succeeded = true;
            FormResultData.ErrorList = [ "Passkey name saved." ];
            ResultComponent.SetFormResult(FormResultData, 2);
        });
    }

    private async Task DeleteAsync(WebAuthCredentialModel item)
    {
        await ExecuteAsync(async () =>
        {
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            var result = await AccountService.DeleteWebAuthCredentialAsync(item.Id, cts.Token);
            if (!result.Succeeded)
            {
                FormResultData.Succeeded = false;
                FormResultData.ErrorList = [ result.ErrorList?.FirstOrDefault() ?? "Unable to delete passkey." ];
                ResultComponent.SetFormResult(FormResultData);
                return;
            }

            Credentials.RemoveAll(c => c.Id == item.Id);
            FormResultData.Succeeded = true;
            FormResultData.ErrorList = [ "Passkey deleted." ];
            ResultComponent.SetFormResult(FormResultData, 2);
        });
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        try
        {
            IsBusy = true;
            ResultComponent.ClearFormResult();
            await action();
        }
        catch (Exception ex)
        {
            FormResultData.Succeeded = false;
            FormResultData.ErrorList = [ $"Error: {ex.Message}" ];
            ResultComponent?.SetFormResult(FormResultData);
        }
        finally
        {
            IsBusy = false;
            StateHasChanged();
        }
    }

    private string ShortId(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length <= 14)
        {
            return id;
        }

        return $"{id[..7]}...{id[^7..]}";
    }
}
