namespace Organization.Blazor.Layout;

/// <summary>
/// Displays backend and frontend versions and whether they match expected shared versions.
/// </summary>
partial class VersionComponent
{
    private FormResultComponent StatusResult { get; set; } = default!;
    private VersionHelper? ServiceVersion { get; set; }
    private bool IsLoading { get; set; }

    private string ApiVersionDisplay => ServiceVersion?.ApiVersion ?? "n/a";
    private string BlazorVersionDisplay => ServiceVersion?.BlazorVersion ?? "n/a";

    private bool ApiMatch => ServiceVersion?.IsApiVersionCompatible() ?? false;
    private bool BlazorMatch => ServiceVersion?.IsBlazorVersionCompatible() ?? false;

    private string ApiStatusText => ApiMatch ? "match" : "mismatch";
    private string BlazorStatusText => BlazorMatch ? "match" : "mismatch";

    /// <summary>
    /// Set this true to display the full version information.
    /// </summary>
    [Parameter] public bool ShowFullVersionInfo { get; set; } = true;
    [Inject] private IVersionService VersionService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Refreshes the version information by calling the API again. This method can be triggered by a user action, such as clicking a "Refresh" button, to update the displayed version information and status indicators based on the latest data from the backend. It ensures that users can verify the current compatibility
    /// of the frontend and backend components without needing to reload the entire application.
    /// </summary>
    private async Task RefreshVersionInfoAsync()
    {
        await LoadVersionInfoAsync();
    }

    /// <summary>
    /// Loads version information from the API and prepares comparison values.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        await LoadVersionInfoAsync();
    }

    /// <summary>
    /// Loads version information from the API and updates the component state. This method is responsible for making the API call to retrieve the version information, handling the loading state, and updating the component's properties with the retrieved data. It also handles any errors that may occur during the API call and updates
    /// the StatusResult component with appropriate messages if the API call fails. This ensures that users are informed about the success or failure of the version retrieval process and can see the most up-to-date version information when it is successfully retrieved.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadVersionInfoAsync()
    {
        IsLoading = true;
        StateHasChanged();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var (data, formResult) = await VersionService.GetVersionAsync(cts.Token);

        ServiceVersion = data;
        if (formResult != null)
        {
            StatusResult.SetFormResult(formResult);
        }

        IsLoading = false;
        StateHasChanged();
    }

    /// <summary>
    /// Do a full refresh of the application.
    /// </summary>
    private async Task FullRefresh()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("organizationApp.hardRefresh", NavigationManager.Uri);
        }
        catch (JSException)
        {
            NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
        }
    }
}