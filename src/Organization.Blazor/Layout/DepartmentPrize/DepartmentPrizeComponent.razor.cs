
using Microsoft.JSInterop;

namespace Organization.Blazor.Layout.DepartmentPrize;

partial class DepartmentPrizeComponent
{
	private FormResultComponent FormResultComponent { get; set; } = null!;
	private bool DisplayDetails { get; set; } = false;
	private bool DisableSubmit { get; set; } = false;
	private bool DisableDelete { get; set; } = true;
	private bool DisableName { get; set; } = false;
	private bool DisableDescription { get; set; } = false;
	private bool DisablePointsCost { get; set; } = false;
	private bool DisableAssignedUser { get; set; } = false;
	private bool DisableStatus { get; set; } = false;
	private bool ShowSpinner { get; set; } = false;
	private string AssignedUserSearchText { get; set; } = string.Empty;
	private bool IsDepartmentAdmin { get; set; } = StaticUserInfoBlazor.DepartmentRole == Shared.RolesEnum.DepartmentAdmin;
	private bool IsOrganizationAdmin { get; set; } = StaticUserInfoBlazor.OrganizationRole == Shared.RolesEnum.OrganizationAdmin;
	private bool IsEnterpriseAdmin { get; set; } = StaticUserInfoBlazor.OrganizationRole == Shared.RolesEnum.EnterpriseAdmin;
	private bool IsPrizeAssignedToMe => ChildContent.AssignedUserId == StaticUserInfoBlazor.User?.Id;
	private string CreatorDisplayName => ChildContent.CreatorUser?.DisplayName ?? ChildContent.CreatorUser?.UserName ?? "Unknown creator";
	private string AddUpdateText => ChildContent.Id == 0 ? "Add Prize" : "Update Prize";
	private (string text, Shared.PrizeStatusEnum enumValue)[] StatusOptions => PrizeWorkflows.GetAvailablePrizeStatus(ChildContent.Status, [StaticUserInfoBlazor.DepartmentRole, StaticUserInfoBlazor.OrganizationRole], IsPrizeAssignedToMe);

	[Parameter] public bool InitDisplayDetails { get; set; } = false;
	[Parameter] public TPrize ChildContent { get; set; } = null!;
	[Parameter] public EventCallback<TPrize> OnPrizeAddedOrUpdatedEvent { get; set; }
	[Parameter] public EventCallback<int> OnPrizeDeletedEvent { get; set; }
	[Parameter] public List<UserModel> UsersWithAccess { get; set; } = [];

	[Inject] private IPrizeService PrizeService { get; set; } = null!;
	[Inject] private IJSRuntime JsRuntime { get; set; } = null!;

	private void SetDisableProperties()
	{
		var isAdmin = IsDepartmentAdmin || IsOrganizationAdmin || IsEnterpriseAdmin;
		var isNewPrize = ChildContent.Id == 0;

		if (ChildContent.Status == Shared.PrizeStatusEnum.Redeemed && !isAdmin)
		{
			DisableName = true;
			DisableDescription = true;
			DisablePointsCost = true;
			DisableAssignedUser = true;
			DisableStatus = true;
			DisableSubmit = true;
			DisableDelete = true;
			return;
		}

		DisableName = !isAdmin;
		DisableDescription = !isAdmin;
		DisablePointsCost = !isAdmin;
		DisableAssignedUser = !isAdmin;
		DisableDelete = isNewPrize || !isAdmin;
		DisableStatus = !isAdmin && !IsPrizeAssignedToMe;

		if (isNewPrize)
		{
			DisableSubmit = false;
		}
		else
		{
			DisableSubmit = DisableName && DisableDescription && DisablePointsCost && DisableAssignedUser && DisableStatus;
		}
	}

	private async Task OnPrizeAddedOrUpdated(TPrize prize)
	{
		DisableSubmit = true;
		ShowSpinner = true;
		FormResultComponent.ClearFormResult();

		// Ensure required ownership context is set for new prizes.
		prize.CreatorUserId ??= StaticUserInfoBlazor.User?.Id ?? string.Empty;
		if (prize.DepartmentId == 0)
		{
			prize.DepartmentId = StaticUserInfoBlazor.SelectedDepartment?.DepartmentId ?? 0;
		}

		using CancellationTokenSource cts = new(TimeSpan.FromSeconds(60));
		var updateResult = await PrizeService.AddUpdatePrizeAsync(prize, cts.Token);

		ShowSpinner = false;
		DisableSubmit = false;

		if (updateResult.formResult != null && !updateResult.formResult.Succeeded)
		{
			FormResultComponent.SetFormResult(updateResult.formResult);
			return;
		}

		if (updateResult.data != null)
		{
			ChildContent = updateResult.data;
			SetDisableProperties();
			FormResultComponent.SetFormResult(new FormResult { Succeeded = true, ErrorList = ["Prize added/updated successfully!"] }, 2);
			await OnPrizeAddedOrUpdatedEvent.InvokeAsync(ChildContent);
			StateHasChanged();
		}
	}

	private async Task SetAssignedUser(string userId)
	{
		FormResultComponent.ClearFormResult();
		if (string.IsNullOrWhiteSpace(userId))
		{
			ChildContent.AssignedUserId = null;
			ChildContent.AssignedUser = null;
			AssignedUserSearchText = string.Empty;
		}
		else
		{
			using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
			var (pointsData, pointsFormResult) = await PrizeService.GetPointsBalanceByUserIdAsync(userId, cts.Token);
			if (pointsFormResult is not null && !pointsFormResult.Succeeded)
			{
				FormResultComponent.SetFormResult(pointsFormResult);
				StateHasChanged();
				return;
			}

			if (pointsData is not null && pointsData.PointsBalance < ChildContent.PointsCost)
			{
				var userName = pointsData.DisplayName ?? pointsData.UserName;
				var confirmMessage = $"{userName} has {pointsData.PointsBalance} available points, but this prize costs {ChildContent.PointsCost}. Are you sure you want to assign this user?";
				var confirmed = await JsRuntime.InvokeAsync<bool>("confirm", confirmMessage);
				if (!confirmed)
				{
					StateHasChanged();
					return;
				}
			}

			var selectedUser = UsersWithAccess.FirstOrDefault(u => u.Id == userId);
			ChildContent.AssignedUserId = userId;
			ChildContent.AssignedUser = selectedUser != null
				? new AppUser { Id = selectedUser.Id, UserName = selectedUser.UserName, DisplayName = selectedUser.DisplayName }
				: null;

			if (selectedUser != null)
			{
				AssignedUserSearchText = GetUserDisplayText(selectedUser);
			}
		}

		StateHasChanged();
	}

	private static string GetUserDisplayText(UserModel user)
		=> $"{user.DisplayName} ({user.UserName})";

	private Task OnAssignedUserSearchChanged(string value)
	{
		AssignedUserSearchText = value;
		return Task.CompletedTask;
	}

	private async Task OnAssignedUserSelected(string selectedText)
	{
		var selected = UsersWithAccess.FirstOrDefault(user =>
			string.Equals(GetUserDisplayText(user), selectedText, StringComparison.OrdinalIgnoreCase));

		if (selected is null)
			return;

		await SetAssignedUser(selected.Id);
	}

	private async Task ClearAssignedUserAsync()
	{
		if (DisableAssignedUser)
			return;

		await SetAssignedUser(string.Empty);
	}

	private async Task DeletePrizeAsync()
	{
		if (ChildContent.Id == 0)
		{
			FormResultComponent.SetFormResult(new FormResult { Succeeded = false, ErrorList = ["Cannot delete a prize that has not been saved yet."] });
			return;
		}

		ShowSpinner = true;
		FormResultComponent.ClearFormResult();

		using CancellationTokenSource cts = new(TimeSpan.FromSeconds(60));
		var deleteResult = await PrizeService.DeletePrizeAsync(ChildContent.Id, cts.Token);

		ShowSpinner = false;
		if (deleteResult.Succeeded)
		{
			await OnPrizeDeletedEvent.InvokeAsync(ChildContent.Id);
			return;
		}

		FormResultComponent.SetFormResult(deleteResult);
	}

	protected override async Task OnInitializedAsync()
	{
		if (!string.IsNullOrEmpty(ChildContent.AssignedUserId))
		{
			var selectedUser = UsersWithAccess.FirstOrDefault(user => user.Id == ChildContent.AssignedUserId);
			if (selectedUser != null)
			{
				AssignedUserSearchText = GetUserDisplayText(selectedUser);
			}
		}

		DisplayDetails = InitDisplayDetails;
		SetDisableProperties();
		await base.OnInitializedAsync();
	}

}
