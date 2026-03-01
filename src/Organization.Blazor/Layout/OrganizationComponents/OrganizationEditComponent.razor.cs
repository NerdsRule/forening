
namespace Organization.Blazor.Layout.OrganizationComponents;

partial class OrganizationEditComponent
{
	private FormResultComponent _formResult { get; set; } = null!;
	private List<TOrganization> _organizations = [];
	private List<TDepartment> _departments = [];
	private TOrganization _organization = new() { IsActive = true };
	private int _selectedOrganizationId;
	private bool _showSpinner;

	[Inject] private IOrganizationService OrganizationService { get; set; } = default!;
	[Inject] private IDepartmentService DepartmentService { get; set; } = default!;

	private string AddUpdateText => _organization.Id == 0 ? "Add" : "Update";

	protected override async Task OnInitializedAsync()
	{
		await LoadOrganizationsAsync();
		await base.OnInitializedAsync();
	}

	private async Task LoadOrganizationsAsync()
	{
		_showSpinner = true;
		var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
		var result = await OrganizationService.GetOrganizationsAsync(ct);
		if (result.formResult is not null)
		{
			_formResult.SetFormResult(result.formResult);
			_organizations = [];
		}
		else
		{
			_organizations = result.data ?? [];
		}
		_showSpinner = false;
	}

	private async Task HandleAddUpdateAsync()
	{
		_formResult.ClearFormResult();
		_showSpinner = true;

		var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
		var result = await OrganizationService.AddUpdateOrganizationAsync(_organization, ct);
		_showSpinner = false;

		if (result.formResult is not null)
		{
			_formResult.SetFormResult(result.formResult);
			return;
		}

		if (result.data is not null)
		{
			var existing = _organizations.FirstOrDefault(o => o.Id == result.data.Id);
			if (existing is null)
			{
				_organizations.Add(result.data);
			}
			else
			{
				existing.Name = result.data.Name;
				existing.Address = result.data.Address;
				existing.ContactEmail = result.data.ContactEmail;
				existing.ContactPhone = result.data.ContactPhone;
				existing.IsActive = result.data.IsActive;
				existing.UpdatedAt = result.data.UpdatedAt;
			}

			_selectedOrganizationId = result.data.Id;
			await LoadDepartmentsByOrganizationIdAsync(_selectedOrganizationId);
			_organization = new TOrganization { IsActive = true };
			_formResult.SetFormResult(new FormResult { Succeeded = true, ErrorList = ["Organization saved"] }, 2);
		}
	}

	private async Task EditOrganization(TOrganization organization)
	{
		_formResult.ClearFormResult();
		_organization = new TOrganization
		{
			Id = organization.Id,
			Name = organization.Name,
			Address = organization.Address,
			ContactEmail = organization.ContactEmail,
			ContactPhone = organization.ContactPhone,
			IsActive = organization.IsActive,
			CreatedAt = organization.CreatedAt,
			UpdatedAt = organization.UpdatedAt
		};
		_selectedOrganizationId = organization.Id;
		await LoadDepartmentsByOrganizationIdAsync(_selectedOrganizationId);
	}

	private void NewOrganization()
	{
		_formResult.ClearFormResult();
		_selectedOrganizationId = 0;
		_departments = [];
		_organization = new TOrganization { IsActive = true };
	}

	private async Task DeleteOrganizationAsync(int id)
	{
		_formResult.ClearFormResult();
		_showSpinner = true;

		var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
		var result = await OrganizationService.DeleteOrganizationAsync(id, ct);
		_showSpinner = false;

		if (!result.Succeeded)
		{
			_formResult.SetFormResult(result);
			return;
		}

		_organizations.RemoveAll(o => o.Id == id);
		if (_organization.Id == id)
		{
			_organization = new TOrganization { IsActive = true };
		}
		if (_selectedOrganizationId == id)
		{
			_selectedOrganizationId = 0;
			_departments = [];
		}
		_formResult.SetFormResult(new FormResult { Succeeded = true, ErrorList = ["Organization deleted"] }, 2);
	}

	private async Task LoadDepartmentsByOrganizationIdAsync(int organizationId)
	{
		if (organizationId <= 0)
		{
			_departments = [];
			return;
		}

		if (StaticUserInfoBlazor.User is null)
		{
			_departments = [];
			_formResult.SetFormResult(new FormResult { Succeeded = false, ErrorList = ["User not loaded"] });
			return;
		}

		_showSpinner = true;
		var ct = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;
		var result = await DepartmentService.GetDepartmentsByOrganizationIdAsync(StaticUserInfoBlazor.User.Id, organizationId, ct);
		_showSpinner = false;

		if (result.formResult is not null)
		{
			_departments = [];
			_formResult.SetFormResult(result.formResult);
			return;
		}

		_departments = result.data ?? [];
	}

	private Task HandleDepartmentAddedAsync(TDepartment department)
	{
		if (_departments.All(d => d.Id != department.Id))
		{
			_departments.Add(department);
		}

		_formResult.SetFormResult(new FormResult { Succeeded = true, ErrorList = [$"Department added: {department.Name}"] }, 2);
		return Task.CompletedTask;
	}

}
