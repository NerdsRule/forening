namespace Organization.Blazor.Pages;

partial class EmailConfirmed
{
    private string? ResolvedToken { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "token")]
    public string? Token { get; set; }

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    protected override void OnParametersSet()
    {
        ResolvedToken = Token;

        if (!string.IsNullOrWhiteSpace(ResolvedToken))
        {
            return;
        }

        var absoluteUri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var query = absoluteUri.Query;
        if (string.IsNullOrWhiteSpace(query))
        {
            return;
        }

        foreach (var queryPart in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var keyValuePair = queryPart.Split('=', 2);
            if (keyValuePair.Length != 2)
            {
                continue;
            }

            if (!string.Equals(keyValuePair[0], "token", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            ResolvedToken = Uri.UnescapeDataString(keyValuePair[1]);
            return;
        }
    }
}