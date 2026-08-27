using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace VK.Blocks.Testing;

/// <summary>
/// A minimal empty implementation of <see cref="IConfiguration"/> used as the default configuration for test fixtures.
/// </summary>
internal sealed class VKEmptyConfiguration : IConfiguration
{
    public static readonly VKEmptyConfiguration Instance = new();

    private VKEmptyConfiguration() { }

    public string? this[string key]
    {
        get => null;
        set { }
    }

    public IEnumerable<IConfigurationSection> GetChildren() => [];

    public IChangeToken GetReloadToken() => NullChangeToken.Instance;

    public IConfigurationSection GetSection(string key) => new VKEmptyConfigurationSection(key);

    private sealed class NullChangeToken : IChangeToken
    {
        public static readonly NullChangeToken Instance = new();
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => NullDisposable.Instance;
    }

    private sealed class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();
        public void Dispose() { }
    }

    private sealed class VKEmptyConfigurationSection(string key) : IConfigurationSection
    {
        public string? this[string subKey]
        {
            get => null;
            set { }
        }

        public string Key { get; } = key;
        public string Path { get; } = key;
        public string? Value { get; set; }

        public IEnumerable<IConfigurationSection> GetChildren() => [];
        public IChangeToken GetReloadToken() => NullChangeToken.Instance;
        public IConfigurationSection GetSection(string subKey) => new VKEmptyConfigurationSection($"{Path}:{subKey}");
    }
}
