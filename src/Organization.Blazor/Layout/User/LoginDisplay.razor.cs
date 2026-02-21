using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Organization.Blazor.Layout.User;

partial class LoginDisplay
{
    
    [Inject] NavigationManager Navigation { get; set; } = null!;
    [Inject] IDepartmentTaskService DepartmentTaskService { get; set; } = null!;
    public void BeginLogOut()
    {
        Console.WriteLine("LoginDisplay: Logging out...");
        Navigation.NavigateToLogout("authentication/logout");
    }
    
    public void MyProfileClick()
    {
        Navigation.NavigateTo("/myprofile");
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
}
