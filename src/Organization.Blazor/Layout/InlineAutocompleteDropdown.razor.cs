namespace Organization.Blazor.Layout;

partial class InlineAutocompleteDropdown<TItem>
{
    [Parameter] public string Id { get; set; } = string.Empty;
    [Parameter] public string Placeholder { get; set; } = string.Empty;
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public int MaxSuggestions { get; set; } = 8;
    [Parameter] public IReadOnlyList<TItem> Items { get; set; } = [];
    [Parameter] public Func<TItem, string>? ItemTextSelector { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public EventCallback<string> OnItemSelected { get; set; }

    private bool IsDropdownOpen { get; set; }

    private List<TItem> FilteredItems =>
        Items
            .Where(item =>
            {
                var text = GetItemText(item);
                return text.Length > 0 &&
                       text.Contains(Value ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
                       !string.Equals(text, Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            })
            .DistinctBy(GetItemText)
            .Take(MaxSuggestions)
            .ToList();

    private string GetItemText(TItem item) => ItemTextSelector?.Invoke(item) ?? item?.ToString() ?? string.Empty;

    private void OpenDropdown(FocusEventArgs _)
    {
        IsDropdownOpen = true;
    }

    private async Task OnInputChanged(ChangeEventArgs e)
    {
        Value = e.Value?.ToString() ?? string.Empty;
        IsDropdownOpen = true;
        await ValueChanged.InvokeAsync(Value);
    }

    private void CloseDropdown(FocusEventArgs _)
    {
        IsDropdownOpen = false;
    }

    private async Task SelectItem(string selected)
    {
        Value = selected;
        IsDropdownOpen = false;
        await ValueChanged.InvokeAsync(Value);
        await OnItemSelected.InvokeAsync(selected);
    }
}
