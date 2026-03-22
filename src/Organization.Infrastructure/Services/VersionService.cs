namespace Organization.Infrastructure.Services;

/// <summary>
/// Service for calling version-related API endpoints.
/// </summary>
/// <param name="httpClientFactory">HTTP client factory.</param>
/// <param name="logger">Logger instance.</param>
public class VersionService(IHttpClientFactory httpClientFactory, ILogger<VersionService> logger) : IVersionService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Auth");

    /// <inheritdoc />
    public async Task<(VersionHelper? data, FormResult? formResult)> GetVersionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("/v1/api/version", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var version = await response.Content.ReadFromJsonAsync<VersionHelper>(_jsonSerializerOptions, cancellationToken);
                return (version, null);
            }

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>(_jsonSerializerOptions, cancellationToken);
            return (null, formResult ?? new FormResult { Succeeded = false, ErrorList = ["Failed to retrieve version info"] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving version info.");
            return (null, new FormResult { Succeeded = false, ErrorList = ["Error retrieving version info"] });
        }
    }
}