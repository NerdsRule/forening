namespace Organization.Infrastructure.Services;

/// <summary>
/// Service for password reset API flows.
/// </summary>
/// <param name="httpClientFactory">HTTP client factory.</param>
/// <param name="logger">Logger instance.</param>
public class ResetPasswordService(IHttpClientFactory httpClientFactory, ILogger<ResetPasswordService> logger) : IResetPasswordService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Auth");

    /// <inheritdoc />
    public Task<FormResult> RequestPasswordResetAsync(RequestPasswordResetModel model)
    {
        return PostFormResultAsync("/v1/api/users/password/requestOfResetPassword", model, "An unknown error prevented the password reset request from succeeding.");
    }

    /// <inheritdoc />
    public Task<FormResult> ResetOwnPasswordAsync(SelfResetPasswordModel model)
    {
        return PostFormResultAsync("/v1/api/users/password/reset/self", model, "An unknown error prevented the password reset from succeeding.");
    }

    /// <inheritdoc />
    public Task<FormResult> ResetPasswordAsync(ResetPasswordModel model)
    {
        return PostFormResultAsync("/v1/api/users/password/reset", model, "An unknown error prevented the password reset from succeeding.");
    }

    /// <inheritdoc />
    public async Task<List<TResetPassword>?> GetResetRequestsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/v1/api/users/password/reset-requests", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<TResetPassword>>(_jsonSerializerOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving reset requests.");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<FormResult> DeleteResetRequestAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/v1/api/users/password/reset-request/{id}", cancellationToken);
            if (response.Content.Headers.ContentLength is null or 0)
            {
                return new FormResult { Succeeded = response.IsSuccessStatusCode };
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            if (formResult is not null)
            {
                return formResult;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while deleting reset request {Id}.", id);
        }

        return new FormResult { Succeeded = false, ErrorList = ["An unknown error prevented the reset request deletion from succeeding."] };
    }

    private async Task<FormResult> PostFormResultAsync<TModel>(string uri, TModel model, string fallbackError)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(uri, model);
            if (response.Content.Headers.ContentLength is null or 0)
            {
                return new FormResult { Succeeded = response.IsSuccessStatusCode };
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions);
            if (formResult is not null)
            {
                return formResult;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while posting password reset request to {Uri}.", uri);
        }

        return new FormResult
        {
            Succeeded = false,
            ErrorList = [fallbackError]
        };
    }
}