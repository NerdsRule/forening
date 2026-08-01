namespace Organization.Infrastructure.Services;

/// <summary>
/// Service for calling user budget API endpoints.
/// </summary>
/// <summary>
/// Service for calling user budget API endpoints.
/// </summary>
public class UserBudgetService(IHttpClientFactory httpClientFactory, ILogger<UserBudgetService> logger) : IUserBudgetService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Auth");

    /// <summary>
    /// Retrieves the current user's budget entries for a department.
    /// </summary>
    /// <param name="departmentId">The department identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The budget entries or a form result containing an error.</returns>
    public async Task<(List<TUserBudget>? data, FormResult? formResult)> GetMyBudgetAsync(int departmentId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v1/api/UserBudget/My/{departmentId}", cancellationToken);
            return await ReadListResponseAsync(response, "Failed to retrieve budget", cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving current user budget for department {DepartmentId}.", departmentId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving budget"] });
        }
    }

    /// <summary>
    /// Retrieves the budget entries for a specific user within a department.
    /// </summary>
    /// <param name="departmentId">The department identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The budget entries or a form result containing an error.</returns>
    public async Task<(List<TUserBudget>? data, FormResult? formResult)> GetUserBudgetAsync(int departmentId, string userId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v1/api/UserBudget/User/{departmentId}/{userId}", cancellationToken);
            return await ReadListResponseAsync(response, "Failed to retrieve user budget", cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving budget for user {UserId} in department {DepartmentId}.", userId, departmentId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving user budget"] });
        }
    }

    /// <summary>
    /// Retrieves all budget entries for a department.
    /// </summary>
    /// <param name="departmentId">The department identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The department budget entries or a form result containing an error.</returns>
    public async Task<(List<TUserBudget>? data, FormResult? formResult)> GetDepartmentBudgetsAsync(int departmentId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v1/api/UserBudget/Department/{departmentId}", cancellationToken);
            return await ReadListResponseAsync(response, "Failed to retrieve department budgets", cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving department budgets for department {DepartmentId}.", departmentId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving department budgets"] });
        }
    }

    /// <summary>
    /// Creates or updates a budget entry.
    /// </summary>
    /// <param name="budget">The budget entry to create or update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated budget entry or a form result containing an error.</returns>
    public async Task<(TUserBudget? data, FormResult? formResult)> AddUpdateBudgetAsync(TUserBudget budget, CancellationToken cancellationToken)
    {
        try
        {
            if (budget is null)
            {
                return (null, new FormResult { Succeeded = false, ErrorList = ["Budget entry cannot be null"] });
            }

            var json = JsonSerializer.Serialize(budget, _jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/v1/api/UserBudget", content, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var updated = await response.Content.ReadFromJsonAsync<TUserBudget>(_jsonSerializerOptions, cancellationToken);
                return (updated, null);
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to add or update budget entry"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while adding/updating budget entry {BudgetId}.", budget?.Id);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error adding/updating budget entry"] });
        }
    }

    /// <summary>
    /// Deletes a budget entry by identifier.
    /// </summary>
    /// <param name="id">The budget entry identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A form result indicating success or failure.</returns>
    public async Task<FormResult> DeleteBudgetAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/v1/api/UserBudget/{id}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                if (response.Content.Headers.ContentLength is null or 0)
                    return new FormResult { Succeeded = true };

                var result = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
                return result ?? new FormResult { Succeeded = true };
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            return formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to delete budget entry"] };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while deleting budget entry {BudgetId}.", id);
            return new FormResult { Succeeded = false, ErrorList = ["Error deleting budget entry"] };
        }
    }

    /// <summary>
    /// Retrieves distinct budget tags for a department.
    /// </summary>
    /// <param name="departmentId">The department identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The department tags or a form result containing an error.</returns>
    public async Task<(List<string>? data, FormResult? formResult)> GetDistinctBudgetTagsByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v1/api/UserBudget/Tags/ByDepartment/{departmentId}", cancellationToken);
            return await ReadStringListResponseAsync(response, "Failed to retrieve budget tags", cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving budget tags for department {DepartmentId}.", departmentId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving budget tags"] });
        }
    }

    /// <summary>
    /// Retrieves budget description suggestions for a department.
    /// </summary>
    /// <param name="departmentId">The department identifier.</param>
    /// <param name="query">The text to use as a suggestion filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching description suggestions or a form result containing an error.</returns>
    public async Task<(List<string>? data, FormResult? formResult)> GetBudgetDescriptionSuggestionsAsync(int departmentId, string query, CancellationToken cancellationToken)
    {
        try
        {
            var encodedQuery = Uri.EscapeDataString(query ?? string.Empty);
            var response = await _httpClient.GetAsync($"/v1/api/UserBudget/Descriptions/ByDepartment/{departmentId}?query={encodedQuery}", cancellationToken);
            return await ReadStringListResponseAsync(response, "Failed to retrieve budget description suggestions", cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving budget description suggestions for department {DepartmentId}.", departmentId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving budget description suggestions"] });
        }
    }

    /// <summary>
    /// Reads an API response containing a list of budget entries.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="fallbackError">The fallback error message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parsed budgets or a form result containing an error.</returns>
    private async Task<(List<TUserBudget>? data, FormResult? formResult)> ReadListResponseAsync(HttpResponseMessage response, string fallbackError, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var budgets = await response.Content.ReadFromJsonAsync<List<TUserBudget>>(_jsonSerializerOptions, cancellationToken);
            return (budgets, null);
        }

        var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
        return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = [fallbackError] });
    }

    /// <summary>
    /// Reads an API response containing a list of string values.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="fallbackError">The fallback error message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parsed values or a form result containing an error.</returns>
    private async Task<(List<string>? data, FormResult? formResult)> ReadStringListResponseAsync(HttpResponseMessage response, string fallbackError, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var values = await response.Content.ReadFromJsonAsync<List<string>>(_jsonSerializerOptions, cancellationToken);
            return (values, null);
        }

        var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
        return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = [fallbackError] });
    }
}