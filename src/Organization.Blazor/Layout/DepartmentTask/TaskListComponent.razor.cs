
namespace Organization.Blazor.Layout.DepartmentTask;

partial class TaskListComponent
{
    private List<TTask> _tasks { get; set; } = new List<TTask>();

    /// <summary>
    /// Load data from the API after the component is initialized
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
    
        await base.OnInitializedAsync();
    }

    
    [Inject] NavigationManager Navigation { get; set; } = null!;
    [Inject] ILocalStorageService LocalStorageService { get; set; } = null!;
    [Inject] private IAccountService AccountService { get; set; } = default!;
    
}
