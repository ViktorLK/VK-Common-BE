namespace VK.Blocks.Observability.OpenTelemetry.Providers;

/// <summary>
/// Provides an abstraction over system environment variables to facilitate unit testing.
/// </summary>
public interface IEnvironmentProvider
{
    /// <summary>
    /// Retrieves the value of an environment variable from the current process.
    /// </summary>
    /// <param name="name">The name of the environment variable.</param>
    /// <returns>The value of the environment variable, or null if it's not found.</returns>
    string? GetVariable(string name);
}
