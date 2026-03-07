namespace Organization.Blazor.Layout.DepartmentPrize;

/// <summary>
/// Prize list component for the landing page. This component displays prizes for the selected department.
/// </summary>
partial class LandingPagePriceComponent
{
    private PrizeListComponent _prizeListComponent = null!;
    private FormResultComponent FormResult { get; set; } = null!;
    private bool ShowSpinner { get; set; } = false;
    private List<UserModel> UsersWithAccess => [];

    /// <summary>
    /// Load data from the API after the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
}
