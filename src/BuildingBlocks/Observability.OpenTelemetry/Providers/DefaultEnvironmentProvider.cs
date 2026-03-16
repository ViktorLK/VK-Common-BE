namespace VK.Blocks.Observability.OpenTelemetry.Providers;

/// <summary>
/// Default implementation of <see cref="IEnvironmentProvider"/> that reads from <see cref="System.Environment"/>.
/// </summary>
public sealed class DefaultEnvironmentProvider : IEnvironmentProvider
{
    /// <inheritdoc />
    public string? GetVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }
}
