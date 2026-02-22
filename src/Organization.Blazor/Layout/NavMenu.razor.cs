
namespace Organization.Blazor.Layout;

partial class NavMenu
{

    [Inject] IAccountService Acct { get; set; } = default!;
    [Inject] IUiStateService UiStateService { get; set; } = null!;

    /// <summary>
    ///  Update component when event is triggered in the UiStateService. This method will be called when the UserUpdatedEvent is triggered in the UiStateService, which can happen when the user's information is updated (e.g., after logging in or out). The StateHasChanged method is called to trigger a re-render of the component, ensuring that the navigation menu reflects the current user's state (e.g., showing different menu options based on whether the user is logged in or not).
    /// </summary>
    /// <returns></returns>
    private void OnUserUpdated()
    {
        StateHasChanged();
    }

    /// <summary>
    ///  OnInitializedAsync is a lifecycle method in Blazor that is called when the component is initialized. In this method, you can perform any necessary setup or initialization tasks for the component. In this case, we are subscribing to the UserUpdatedEvent in the UiStateService, so that whenever the user's information is updated, the OnUserUpdated method will be called to update the navigation menu accordingly. We also call the base implementation of OnInitializedAsync to ensure that any additional initialization logic defined in the base class is executed.
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        UiStateService.UserUpdatedEvent += OnUserUpdated;
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Dispose is a method that is called when the component is being removed from the UI. In this method, you can perform any necessary cleanup tasks, such as unsubscribing from events or
    /// releasing resources. In this case, we are unsubscribing from the UserUpdatedEvent in the UiStateService to prevent memory leaks and ensure that the component does not continue to receive updates after it has been disposed.
    /// </summary> <returns></returns>
    public void Dispose()
    {        
        UiStateService.UserUpdatedEvent -= OnUserUpdated;
    }

}
