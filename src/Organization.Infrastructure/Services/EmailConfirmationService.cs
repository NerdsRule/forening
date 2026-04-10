namespace Organization.Infrastructure.Services;

/// <summary>
/// Service for email confirmation API flows.
/// </summary>
/// <param name="httpClientFactory">HTTP client factory.</param>
/// <param name="logger">Logger instance.</param>
public class EmailConfirmationService(IHttpClientFactory httpClientFactory, ILogger<EmailConfirmationService> logger) : IEmailConfirmationService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Auth");

    /// <inheritdoc />
    public Task<FormResult> RequestEmailConfirmationTokenAsync(CancellationToken cancellationToken = default)
    {
        return PostFormResultAsync<object>("/v1/api/users/email-confirmation/request-token", null, "An unknown error prevented the email confirmation request from succeeding.", cancellationToken);
    }

    /// <inheritdoc />
    public Task<FormResult> ConfirmEmailAsync(EmailConfirmationConfirmModel model, CancellationToken cancellationToken = default)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Token))
        {
            return Task.FromResult(new FormResult
            {
                Succeeded = false,
                ErrorList = ["Token is required."]
            });
        }

        return PostFormResultAsync("/v1/api/users/email-confirmation/confirm", model, "An unknown error prevented the email confirmation from succeeding.", cancellationToken);
    }

    /// <summary>
    /// Sends a POST request and converts the response to a <see cref="FormResult"/>.
    /// </summary>
    /// <param name="uri">Endpoint URI to post to.</param>
    /// <param name="model">Optional request payload.</param>
    /// <param name="fallbackError">Fallback error text when no response payload can be parsed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parsed operation result or a fallback failure result.</returns>
    private async Task<FormResult> PostFormResultAsync<TModel>(string uri, TModel? model, string fallbackError, CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = model is null
                ? await _httpClient.PostAsync(uri, content: null, cancellationToken)
                : await _httpClient.PostAsJsonAsync(uri, model, cancellationToken);

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
            logger.LogError(ex, "Error while posting email confirmation request to {Uri}.", uri);
        }

        return new FormResult
        {
            Succeeded = false,
            ErrorList = [fallbackError]
        };
    }
}