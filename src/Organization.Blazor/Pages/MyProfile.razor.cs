

namespace Organization.Blazor.Pages;

partial class MyProfile 
{

    private void NavigateToChangePassword()
    {
        Navigation.NavigateTo("/Authentication/ChangePassword");
    }
    [Inject] NavigationManager Navigation { get; set; } = null!;
}
