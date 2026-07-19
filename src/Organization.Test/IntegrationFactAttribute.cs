namespace Organization.Test;

internal static class IntegrationTestSettings
{
    public const int DefaultTimeoutMs = 240_000;

    public static bool RunIntegrationTests =>
        string.Equals(Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS"), "true", StringComparison.OrdinalIgnoreCase);
}
