namespace Organization.Blazor.Pages;

public partial class PrizeEdit
{
    private PrizeListComponent _prizeListComponent = null!;
    private FormResultComponent FormResult { get; set; } = null!;
    private TPrize _newPrize { get; set; } = new();
    private bool ShowSpinner { get; set; } = true;
    private List<UserModel> UsersWithAccessToOrganization { get; set; } = [];
    private List<UserModel> UsersWithAccessToDepartment { get; set; } = [];
    private List<UserModel> UsersWithAccess => [.. UsersWithAccessToOrganization.Union(UsersWithAccessToDepartment)];

    [Inject] NavigationManager Navigation { get; set; } = null!;
    [Inject] IDepartmentTaskService DepartmentTaskService { get; set; } = null!;

    private async Task OnPrizeAddedOrUpdated(TPrize prize)
    {
        BuildEmptyPrize();
        _prizeListComponent.AddPrizeToList(prize);
    }

    private void BuildEmptyPrize()
    {
        _newPrize = new TPrize
        {
            CreatorUserId = StaticUserInfoBlazor.User!.Id,
            CreatorUser = new AppUser
            {
                Id = StaticUserInfoBlazor.User.Id,
                UserName = StaticUserInfoBlazor.User.DisplayName ?? StaticUserInfoBlazor.User.UserName,
                DisplayName = StaticUserInfoBlazor.User.DisplayName ?? StaticUserInfoBlazor.User.UserName,
            },
            DepartmentId = StaticUserInfoBlazor.SelectedDepartment!.DepartmentId,
            Status = Shared.PrizeStatusEnum.Available,
        };
    }

    protected override async Task OnInitializedAsync()
    {
        if (FormResult is not null)
        {
            FormResult.SetFormResult(new FormResult { Succeeded = true, ErrorList = ["Loading user info..."] });
        }

        ShowSpinner = true;
        if (StaticUserInfoBlazor.User is null)
        {
            Navigation.NavigateTo("/");
            return;
        }

        BuildEmptyPrize();

        if (StaticUserInfoBlazor.OrganizationRole == Shared.RolesEnum.OrganizationAdmin || StaticUserInfoBlazor.DepartmentRole == Shared.RolesEnum.EnterpriseAdmin)
        {
            var (usersWithAccessToOrganization, formResultUsersWithAccessToOrganization) =
                await DepartmentTaskService.GetUsersWithAccessToOrganizationAsync(StaticUserInfoBlazor.SelectedOrganization!.Id, CancellationToken.None);
            if (formResultUsersWithAccessToOrganization is not null && FormResult is not null)
            {
                FormResult.SetFormResult(formResultUsersWithAccessToOrganization);
            }
            else if (usersWithAccessToOrganization is not null)
            {
                UsersWithAccessToOrganization = usersWithAccessToOrganization;
            }
        }

        var (usersWithAccessToDepartment, formResultUsersWithAccessToDepartment) =
            await DepartmentTaskService.GetUsersWithAccessToDepartmentAsync(StaticUserInfoBlazor.SelectedDepartment!.DepartmentId, CancellationToken.None);
        if (formResultUsersWithAccessToDepartment is not null && FormResult is not null)
        {
            FormResult.SetFormResult(formResultUsersWithAccessToDepartment);
        }
        else if (usersWithAccessToDepartment is not null)
        {
            UsersWithAccessToDepartment = usersWithAccessToDepartment;
        }

        ShowSpinner = false;
        if (FormResult is not null)
        {
            FormResult.ClearFormResult();
        }

        await base.OnInitializedAsync();
    }
}
