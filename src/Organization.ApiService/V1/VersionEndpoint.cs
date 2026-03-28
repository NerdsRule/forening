namespace Organization.ApiService.V1;

/// <summary>
/// Version endpoints.
/// </summary>
public static class VersionEndpoint
{
    /// <summary>
    /// Maps version-related HTTP endpoints to the provided <see cref="WebApplication"/> instance.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to which endpoints will be mapped.</param>
    public static void MapVersionEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/v1");

        /// <summary>
        /// Returns the current API and Blazor versions.
        /// </summary>
        /// <returns>The current version information.</returns>
        v1.MapGet("/api/version", (HttpContext ctx) =>
        {
            var apiVersion = VersionHelper.GetAssemblyVersion(
                System.Reflection.Assembly.GetExecutingAssembly());
            return Results.Ok(new VersionHelper { ApiVersion = apiVersion });
        })
            .Produces<VersionHelper>(StatusCodes.Status200OK)
            .AllowAnonymous();
    }
}