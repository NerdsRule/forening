
namespace Organization.Blazor.Pages;

public partial class TaskEdit
{
    private TTask _newTask { get; set; } = new TTask();

/// <summary>
    /// Load data from the API after the component is initialized
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Load user info
        _ = await AccountService.CheckAuthenticatedAsync();
        if (StaticUserInfoBlazor.User is null)
        {
            Navigation.NavigateTo("/");
            return;
        }
        await base.OnInitializedAsync();
    }
    [Inject] NavigationManager Navigation { get; set; } = null!;
    [Inject] ILocalStorageService LocalStorageService { get; set; } = null!;
    [Inject] private IAccountService AccountService { get; set; } = default!;
}
