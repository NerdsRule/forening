
namespace Organization.Shared.Helpers;

/// <summary>
/// Static class to hold shared API service related helper methods or constants. This can be used across the solution to avoid duplication and centralize API service logic.
/// </summary>
public static class ApiServiceStatic
{

    /// <summary>
    /// Array of strings with allowed CORS origins for development. This can be overridden by environment variable "CORS" which should contain a semicolon-separated list of origins.
    /// </summary>
    public static string[] AllowedOrigins = [];

    /// <summary>
    /// HashSet of allowed origins for efficient lookup. This is derived from the AllowedOrigins array and can be used in CORS policy or other places where we need to check if an origin is allowed.
    /// </summary>
    public static HashSet<string> AllowedOriginsSet => [.. AllowedOrigins];
    
}
