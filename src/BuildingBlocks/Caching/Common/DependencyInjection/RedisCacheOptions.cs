namespace VK.Blocks.Caching;

/// <summary>
/// Redis specific configuration.
/// </summary>
public sealed record RedisCacheOptions
{
    public string Configuration { get; init; } = "localhost";
    public string InstanceName { get; init; } = string.Empty;
}
