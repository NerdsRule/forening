using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Organization.Blazor.Layout.User;

partial class LoginDisplay
{
    
    [Inject] NavigationManager Navigation { get; set; } = null!;
    [Inject] IDepartmentTaskService DepartmentTaskService { get; set; } = null!;
    [Inject] IUiStateService UiStateService { get; set; } = null!;

    /// <summary>
    /// BeginLogOut is a method that will be called when the user clicks on the "Logout" link in the login display. It will navigate the user to the logout page, which will handle the logout process and redirect the user back to the home page or login page after logging out. The Console.WriteLine statement is included for debugging purposes, allowing you to see in the console when the logout process is initiated.
    /// </summary>
    public void BeginLogOut()
    {
        Console.WriteLine("LoginDisplay: Logging out...");
        Navigation.NavigateToLogout("authentication/logout");
    }
    
    /// <summary>
    /// Navigate to the user's profile page. This method will be called when the user clicks on the "My Profile" link in the login display. It will navigate the user to the "/myprofile" route, where they can view and edit their profile information.
    /// </summary>
    public void MyProfileClick()
    {
        Navigation.NavigateTo("/myprofile");
    }

    /// <summary>
    /// Update component when event is triggered in the UiStateService. This method will be called when the UserUpdatedEvent is triggered in the UiStateService, which can happen when the user's information is updated (e.g., after logging in or out). The StateHasChanged method is called to trigger a re-render of the component, ensuring that the login display reflects the current user's state (e.g., showing the user's name when logged in or showing the login link when logged out).
    /// </summary> <param name="sender"></param>
    /// <param name="args"></param>
    private void OnUserUpdated()
    {
        StateHasChanged();
    }

    /// <summary>
    /// OnInitializedAsync is a lifecycle method in Blazor that is called when the component is initialized. In this method, you can perform any necessary setup or initialization tasks for the component. In this case, we are simply calling the base implementation of OnInitializedAsync, but you could also add any additional logic here if needed, such as fetching user data or initializing state related to the login display.
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        UiStateService.UserUpdatedEvent += OnUserUpdated;
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Dispose is a method that is called when the component is being removed from the UI. In this method, you can perform any necessary cleanup tasks, such as unsubscribing from events or releasing resources. In this case, we are unsubscribing from the UserUpdatedEvent in the Ui
    /// StateService to prevent memory leaks and ensure that the component does not continue to receive updates after it has been disposed.
    /// </summary> <returns></returns>
    public void Dispose()
    {        
        UiStateService.UserUpdatedEvent -= OnUserUpdated;
    }
}
