

namespace Organization.Blazor.Pages;

partial class MyProfile 
{

    /// <summary>
    /// Form where user change selected department or organization
    /// </summary>
    private OrganizationDepartmentForm _organizationDepartmentForm { get; set; } = new();
    private FormResultComponent FormResult { get; set; } = null!;
    private bool IsUpdating { get; set; } = false;
    [Inject] NavigationManager Navigation { get; set; } = null!;
    [Inject] ILocalStorageService LocalStorageService { get; set; } = null!;
    [Inject] private IAccountService AccountService { get; set; } = null!;
    [Inject] IUiStateService UiStateService { get; set; } = null!;

    /// <summary>
    /// Handle valid form submission
    /// </summary>
    public void HandleValidSubmit()
    {
        StaticUserInfoBlazor.SelectedOrganization = StaticUserInfoBlazor.User!.AppUserOrganizations.FirstOrDefault(o => o.OrganizationId == _organizationDepartmentForm.SelectedOrganizationId);
        StaticUserInfoBlazor.SelectedDepartment = StaticUserInfoBlazor.User!.AppUserDepartments.FirstOrDefault(d => d.DepartmentId == _organizationDepartmentForm.SelectedDepartmentId);
        IsUpdating = true;
        // save to local storage
        var userLocalStorage = new UserLocalStorage
        {
            SelectedOrganizationId = _organizationDepartmentForm.SelectedOrganizationId ?? 0,
            SelectedDepartmentId = _organizationDepartmentForm.SelectedDepartmentId ?? 0,
        };
        LocalStorageService.SetItemAsync(StaticUserInfoBlazor.UserLocalStorageKey, userLocalStorage);
        FormResult.SetFormResult(new FormResult { Succeeded = true, ErrorList = ["Profile updated successfully!"] }, timeoutSeconds: 2);
        UiStateService.NotifyUserUpdated();
        IsUpdating = false;
    }

    /// <summary>
    /// Update _organizationDepartmentForm.Departments when organization is changed
    /// </summary>
    private void OnOrganizationChanged()
    {
        IsUpdating = true;
        _organizationDepartmentForm.SelectedDepartmentId = null;
        _organizationDepartmentForm.SelectedDepartment = null;
        if (_organizationDepartmentForm.SelectedOrganizationId != null)
        {
            var orgId = _organizationDepartmentForm.SelectedOrganizationId.Value;                
            _organizationDepartmentForm.Departments = [.. StaticUserInfoBlazor.User!.AppUserDepartments
                .Where(d => d.AppUserId == StaticUserInfoBlazor.User.Id && d.Department!.OrganizationId == orgId)
                .Select(c => c.Department!)];
        }
        else
        {
            _organizationDepartmentForm.Departments = [];
        }
        FormResult.SetFormResult(new FormResult { Succeeded = true, ErrorList = ["Organization changed. Please select a department."] });
        UiStateService.NotifyUserUpdated();
        IsUpdating = false;
    }

    /// <summary>
    /// Navigate to change password page
    /// </summary>
    private void NavigateToChangePassword()
    {
        Navigation.NavigateTo("/Authentication/ChangePassword");
    }

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
        _organizationDepartmentForm.SelectedOrganization = StaticUserInfoBlazor.SelectedOrganization?.Organization;
        _organizationDepartmentForm.SelectedDepartment = StaticUserInfoBlazor.SelectedDepartment?.Department;
        _organizationDepartmentForm.SelectedOrganizationId = StaticUserInfoBlazor.SelectedOrganization?.OrganizationId;
        _organizationDepartmentForm.SelectedDepartmentId = StaticUserInfoBlazor.SelectedDepartment?.Id;
        _organizationDepartmentForm.Organizations = [.. StaticUserInfoBlazor.User.AppUserOrganizations.Select(c => c.Organization!)];
        _organizationDepartmentForm.Departments = [.. StaticUserInfoBlazor.User.AppUserDepartments.Where(d => d.AppUserId == StaticUserInfoBlazor.User.Id).Select(c => c.Department!)];
        await base.OnInitializedAsync();
    }
    
}
