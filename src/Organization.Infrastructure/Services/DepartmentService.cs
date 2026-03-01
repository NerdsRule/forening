namespace Organization.Infrastructure.Services;

/// <summary>
/// Service for calling department-related API endpoints.
/// </summary>
/// <param name="httpClientFactory">HTTP client factory.</param>
/// <param name="logger">Logger instance.</param>
public class DepartmentService(IHttpClientFactory httpClientFactory, ILogger<DepartmentService> logger) : IDepartmentService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Auth");

    /// <summary>
    /// Retrieves departments for a given organization and user context.
    /// </summary>
    /// <param name="userId">User id used by the API for access checks.</param>
    /// <param name="organizationId">Organization id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing departments or an API error form result.</returns>
    public async Task<(List<TDepartment>? data, FormResult? formResult)> GetDepartmentsByOrganizationIdAsync(string userId, int organizationId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v1/api/department/{userId}/{organizationId}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var departments = await response.Content.ReadFromJsonAsync<List<TDepartment>>(_jsonSerializerOptions, cancellationToken);
                return (departments, null);
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to retrieve departments"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving departments for organization {OrganizationId} and user {UserId}.", organizationId, userId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving departments"] });
        }
    }

    /// <summary>
    /// Adds or updates a department.
    /// </summary>
    /// <param name="department">Department payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the updated department or an API error form result.</returns>
    public async Task<(TDepartment? data, FormResult? formResult)> AddUpdateDepartmentAsync(TDepartment department, CancellationToken cancellationToken)
    {
        try
        {
            if (department is null)
            {
                return (null, new FormResult { Succeeded = false, ErrorList = ["Department cannot be null"] });
            }

            var json = JsonSerializer.Serialize(department, _jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/v1/api/department", content, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var updated = await response.Content.ReadFromJsonAsync<TDepartment>(_jsonSerializerOptions, cancellationToken);
                return (updated, null);
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to add or update department"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while adding/updating department {DepartmentId}.", department.Id);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error adding/updating department"] });
        }
    }

    /// <summary>
    /// Deletes a department by id.
    /// </summary>
    /// <param name="id">Department id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A form result indicating operation success or failure.</returns>
    public async Task<FormResult> DeleteDepartmentAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/v1/api/department/{id}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                if (response.Content.Headers.ContentLength is null or 0)
                {
                    return new FormResult { Succeeded = true };
                }

                var result = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
                return result ?? new FormResult { Succeeded = true };
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            return formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to delete department"] };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while deleting department {DepartmentId}.", id);
            return new FormResult { Succeeded = false, ErrorList = ["Error deleting department"] };
        }
    }
}