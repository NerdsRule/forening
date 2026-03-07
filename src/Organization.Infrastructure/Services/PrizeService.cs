namespace Organization.Infrastructure.Services;

/// <summary>
/// Service for calling prize-related API endpoints.
/// </summary>
/// <param name="httpClientFactory">HTTP client factory.</param>
/// <param name="logger">Logger instance.</param>
public class PrizeService(IHttpClientFactory httpClientFactory, ILogger<PrizeService> logger) : IPrizeService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Auth");

    /// <inheritdoc />
    public async Task<(List<TPrize>? data, FormResult? formResult)> GetPrizesByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v1/api/price/ByDepartment/{departmentId}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var prizes = await response.Content.ReadFromJsonAsync<List<TPrize>>(_jsonSerializerOptions, cancellationToken);
                return (prizes, null);
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to retrieve prizes"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving prizes for department {DepartmentId}.", departmentId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving prizes"] });
        }
    }

    /// <inheritdoc />
    public async Task<(TPrize? data, FormResult? formResult)> GetPrizeByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v1/api/price/{id}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var prize = await response.Content.ReadFromJsonAsync<TPrize>(_jsonSerializerOptions, cancellationToken);
                return (prize, null);
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to retrieve prize"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving prize {PrizeId}.", id);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving prize"] });
        }
    }

    /// <inheritdoc />
    public async Task<(TPrize? data, FormResult? formResult)> AddUpdatePrizeAsync(TPrize prize, CancellationToken cancellationToken)
    {
        try
        {
            if (prize is null)
            {
                return (null, new FormResult { Succeeded = false, ErrorList = ["Prize cannot be null"] });
            }

            var json = JsonSerializer.Serialize(prize, _jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/v1/api/Price", content, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var updated = await response.Content.ReadFromJsonAsync<TPrize>(_jsonSerializerOptions, cancellationToken);
                return (updated, null);
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to add or update prize"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while adding/updating prize {PrizeId}.", prize.Id);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error adding/updating prize"] });
        }
    }

    /// <inheritdoc />
    public async Task<FormResult> DeletePrizeAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/v1/api/price/{id}", cancellationToken);
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
            return formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to delete prize"] };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while deleting prize {PrizeId}.", id);
            return new FormResult { Succeeded = false, ErrorList = ["Error deleting prize"] };
        }
    }

    /// <inheritdoc />
    public async Task<(UserPointsBalanceModel? data, FormResult? formResult)> GetPointsBalanceByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return (null, new FormResult { Succeeded = false, ErrorList = ["User id cannot be empty"] });
            }

            var response = await _httpClient.GetAsync($"/v1/api/price/PointsBalance/ByUser/{userId}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var points = await response.Content.ReadFromJsonAsync<UserPointsBalanceModel>(_jsonSerializerOptions, cancellationToken);
                return (points, null);
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to retrieve user points balance"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving points balance for user {UserId}.", userId);
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving user points balance"] });
        }
    }
}
