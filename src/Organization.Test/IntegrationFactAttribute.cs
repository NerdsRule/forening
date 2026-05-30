namespace Organization.Test;

/// <summary>
/// Marks a test as integration-only. Tests are skipped unless RUN_INTEGRATION_TESTS=true.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class IntegrationFactAttribute : FactAttribute
{
    private const int DefaultTimeoutMs = 240_000;

    public IntegrationFactAttribute(
        [System.Runtime.CompilerServices.CallerFilePath] string? sourceFilePath = null,
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        : base(sourceFilePath, sourceLineNumber)
    {
        Timeout = DefaultTimeoutMs;

        var runIntegration = Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS");
        if (!string.Equals(runIntegration, "true", StringComparison.OrdinalIgnoreCase))
        {
            Skip = "Integration test. Set RUN_INTEGRATION_TESTS=true to run.";
        }
    }
}
