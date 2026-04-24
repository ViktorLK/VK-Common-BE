using System;

namespace VK.Blocks.Core.Common.Internal;

/// <summary>
/// A default implementation of <see cref="IVKGuidGenerator"/> using <see cref="Guid.NewGuid()"/>.
/// </summary>
internal sealed class DefaultGuidGenerator : IVKGuidGenerator
{
    /// <inheritdoc />
    public Guid Create() => Guid.NewGuid();
}
