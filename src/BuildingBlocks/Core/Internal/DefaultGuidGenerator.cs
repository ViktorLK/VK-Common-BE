using System;
using VK.Blocks.Core.Abstractions;

namespace VK.Blocks.Core.Internal;

/// <summary>
/// A default implementation of <see cref="IGuidGenerator"/> using <see cref="Guid.NewGuid()"/>.
/// </summary>
public sealed class DefaultGuidGenerator : IGuidGenerator
{
    /// <inheritdoc />
    public Guid Create() => Guid.NewGuid();
}
