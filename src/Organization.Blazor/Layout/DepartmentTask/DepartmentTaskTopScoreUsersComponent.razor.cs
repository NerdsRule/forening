
using System.Runtime.InteropServices;

namespace Organization.Blazor.Layout.DepartmentTask;

partial class DepartmentTaskTopScoreUsersComponent
{
    private FormResultComponent FormResult { get; set; } = null!;
    private bool IsLoading { get; set; } = false;
    private List<VTaskPointsAwarded> TopUsersWithPointsAwarded { get; set; } = [];
    private List<VTaskPointsAwarded> TopUsersWithPointsAwardedWithFilter => TopUsersWithPointsAwarded.OrderByDescending(u => u.TaskPointsAwarded).Take(SelectedShowUsersWithPointsAwarded).ToList();
    private int[] ShowUsersWithPointsAwarded = [5, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100];
    private int SelectedShowUsersWithPointsAwarded { get; set; } = 5;
    [Inject] private IDepartmentTaskService DepartmentTaskService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadTopUsersWithPointsAwarded();
    }

    private async Task LoadTopUsersWithPointsAwarded()
    {
        IsLoading = true;
        if (FormResult != null)
            FormResult.ClearFormResult();
        var (data, formResult) = await DepartmentTaskService.GetTopUsersWithPointsAwardedByDepartmentAsync(StaticUserInfoBlazor.User!.Id, StaticUserInfoBlazor.SelectedDepartment!.DepartmentId, 10, CancellationToken.None);
        if (data != null)
        {
            TopUsersWithPointsAwarded = data;
        } else if (formResult != null)
        {
            if (FormResult != null)
                FormResult.SetFormResult(formResult);
        }
        IsLoading = false;
    }
}
