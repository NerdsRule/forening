namespace Organization.Infrastructure.Services;

/// <summary>
/// Service for calling organization-related API endpoints.
/// </summary>
/// <param name="httpClientFactory">HTTP client factory.</param>
/// <param name="logger">Logger instance.</param>
public class OrganizationService(IHttpClientFactory httpClientFactory, ILogger<OrganizationService> logger) : IOrganizationService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Auth");

    /// <summary>
    /// Retrieves all organizations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing a list of organizations or an API error form result.</returns>
    public async Task<(List<TOrganization>? data, FormResult? formResult)> GetOrganizationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("/v1/api/organization/all", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var organizations = await response.Content.ReadFromJsonAsync<List<TOrganization>>(_jsonSerializerOptions, cancellationToken);
                return (organizations, null);
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to retrieve organizations"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving organizations.");
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving organizations"] });
        }
    }

    /// <summary>
    /// Retrieves a single organization by its ID.
    /// </summary>
    /// <param name="id">The ID of the organization to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the organization or an API error form result.</returns>
    public async Task<(TOrganization? data, FormResult? formResult)> GetOrganizationByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v1/api/organization/{id}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var organization = await response.Content.ReadFromJsonAsync<TOrganization>(_jsonSerializerOptions, cancellationToken);
                return (organization, null);
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to retrieve organization"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving organization {OrganizationId}.", id);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving organization"] });
        }
    }

    /// <summary>
    /// Adds or updates an organization.
    /// </summary>
    /// <param name="organization">The organization to add or update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the updated organization or an API error form result.</returns>
    public async Task<(TOrganization? data, FormResult? formResult)> AddUpdateOrganizationAsync(TOrganization organization, CancellationToken cancellationToken)
    {
        try
        {
            if (organization is null)
            {
                return (null, new FormResult { Succeeded = false, ErrorList = ["Organization cannot be null"] });
            }

            var json = JsonSerializer.Serialize(organization, _jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("/v1/api/organization", content, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var updated = await response.Content.ReadFromJsonAsync<TOrganization>(_jsonSerializerOptions, cancellationToken);
                return (updated, null);
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to add or update organization"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while adding/updating organization {OrganizationId}.", organization.Id);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error adding/updating organization"] });
        }
    }

    /// <summary>
    /// Deletes an organization by its ID.
    /// </summary>
    /// <param name="id">The ID of the organization to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A form result indicating the success or failure of the delete operation.</returns>
    public async Task<FormResult> DeleteOrganizationAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/v1/api/organization/{id}", cancellationToken);
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
            return formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to delete organization"] };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while deleting organization {OrganizationId}.", id);
            return new FormResult { Succeeded = false, ErrorList = ["Error deleting organization"] };
        }
    }
}