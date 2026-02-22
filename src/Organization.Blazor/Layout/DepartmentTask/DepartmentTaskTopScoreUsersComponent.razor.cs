
using System.Runtime.InteropServices;

namespace Organization.Blazor.Layout.DepartmentTask;

partial class DepartmentTaskTopScoreUsersComponent
{
    private FormResultComponent FormResult { get; set; } = null!;
    private bool IsLoading { get; set; } = false;
    private List<VTaskPointsAwarded> TopUsersWithPointsAwarded { get; set; } = [];
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
        var (data, formResult) = await DepartmentTaskService.GetTopUsersWithPointsAwardedByDepartmentAsync("userId", 1, 10, CancellationToken.None);
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
