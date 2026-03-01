
namespace Organization.Blazor.Layout.DepartmentComponents;

partial class DepartmentEditComponent
{
	private FormResultComponent _formResult { get; set; } = null!;
	private List<TDepartment> _departments = [];
	private TDepartment _department = new() { IsActive = true };
	private bool _showSpinner;
	private int _lastLoadedOrganizationId;

	[Parameter] public int OrganizationId { get; set; }
	[Parameter] public EventCallback<TDepartment> OnDepartmentAdded { get; set; }

	[Inject] private IDepartmentService DepartmentService { get; set; } = default!;

	private string AddUpdateText => _department.Id == 0 ? "Add" : "Update";

	protected override async Task OnParametersSetAsync()
	{
		if (OrganizationId != _lastLoadedOrganizationId)
		{
			_lastLoadedOrganizationId = OrganizationId;
			await LoadDepartmentsAsync();
			NewDepartment();
		}

		await base.OnParametersSetAsync();
	}

	private async Task LoadDepartmentsAsync()
	{
		if (OrganizationId <= 0)
		{
			_departments = [];
			return;
		}

		if (StaticUserInfoBlazor.User is null)
		{
			_departments = [];
			return;
		}

		_showSpinner = true;
		var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
		var result = await DepartmentService.GetDepartmentsByOrganizationIdAsync(StaticUserInfoBlazor.User.Id, OrganizationId, ct);
		_showSpinner = false;

		if (result.formResult is not null)
		{
			_departments = [];
			if (_formResult is not null)
			{
				_formResult.SetFormResult(result.formResult);
			}
			return;
		}

		_departments = result.data ?? [];
	}

	private async Task HandleAddUpdateAsync()
	{
		_formResult.ClearFormResult();

		if (OrganizationId <= 0)
		{
			_formResult.SetFormResult(new FormResult { Succeeded = false, ErrorList = ["OrganizationId is required"] });
			return;
		}

		_department.OrganizationId = OrganizationId;
		_showSpinner = true;

		var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
		var result = await DepartmentService.AddUpdateDepartmentAsync(_department, ct);
		_showSpinner = false;

		if (result.formResult is not null)
		{
			_formResult.SetFormResult(result.formResult);
			return;
		}

		if (result.data is not null)
		{
			var existing = _departments.FirstOrDefault(d => d.Id == result.data.Id);
			var isNewDepartment = existing is null;
			if (existing is null)
			{
				_departments.Add(result.data);
			}
			else
			{
				existing.Name = result.data.Name;
				existing.Code = result.data.Code;
				existing.Description = result.data.Description;
				existing.IsActive = result.data.IsActive;
				existing.OrganizationId = result.data.OrganizationId;
			}

			if (isNewDepartment)
			{
				await OnDepartmentAdded.InvokeAsync(result.data);
			}

			NewDepartment();
			_formResult.SetFormResult(new FormResult { Succeeded = true, ErrorList = ["Department saved"] }, 2);
		}
	}

	private void EditDepartment(TDepartment department)
	{
		_formResult.ClearFormResult();
		_department = new TDepartment
		{
			Id = department.Id,
			Name = department.Name,
			Code = department.Code,
			Description = department.Description,
			IsActive = department.IsActive,
			OrganizationId = department.OrganizationId
		};
	}

	private void NewDepartment()
	{
		_department = new TDepartment
		{
			OrganizationId = OrganizationId,
			IsActive = true
		};
	}

	private async Task DeleteDepartmentAsync(int id)
	{
		_formResult.ClearFormResult();
		_showSpinner = true;

		var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
		var result = await DepartmentService.DeleteDepartmentAsync(id, ct);
		_showSpinner = false;

		if (!result.Succeeded)
		{
			_formResult.SetFormResult(result);
			return;
		}

		_departments.RemoveAll(d => d.Id == id);
		if (_department.Id == id)
		{
			NewDepartment();
		}

		_formResult.SetFormResult(new FormResult { Succeeded = true, ErrorList = ["Department deleted"] }, 2);
	}

}
