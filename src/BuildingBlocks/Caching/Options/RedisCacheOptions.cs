namespace VK.Blocks.Caching.Options;


/// <summary>
/// Redis specific configuration.
/// </summary>
public sealed class RedisCacheOptions
{
    public string Configuration { get; set; } = "localhost";
    public string InstanceName { get; set; } = string.Empty;
}
