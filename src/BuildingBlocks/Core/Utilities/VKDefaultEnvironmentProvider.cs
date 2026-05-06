using System;

namespace VK.Blocks.Core.Utilities;

/// <summary>
/// Default implementation of <see cref="IVKEnvironmentProvider"/> using <see cref="Environment"/>.
/// </summary>
public sealed class VKDefaultEnvironmentProvider : IVKEnvironmentProvider
{
    /// <inheritdoc />
    public string? GetVariable(string name) => Environment.GetEnvironmentVariable(name);
}
