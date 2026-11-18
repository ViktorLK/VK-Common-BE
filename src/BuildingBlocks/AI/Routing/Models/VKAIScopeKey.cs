using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Represents an opaque, non-empty scope key for AI routing and quota isolation.
/// </summary>
// [AP.01] [FS-04]
public readonly record struct VKAIScopeKey
{
    /// <summary>
    /// Gets the underlying non-empty scope key string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKAIScopeKey"/> struct.
    /// </summary>
    /// <param name="value">The non-empty scope key value.</param>
    public VKAIScopeKey(string value)
    {
        Value = VKGuard.NotNullOrWhiteSpace(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}
