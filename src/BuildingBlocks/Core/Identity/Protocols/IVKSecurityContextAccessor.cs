using System;

namespace VK.Blocks.Core;

/// <summary>
/// Accessor interface for getting, setting, and scoping ambient <see cref="IVKSecurityContext"/> across asynchronous operations.
/// Follows AP.01, AP.03, and CS.01.
/// </summary>
public interface IVKSecurityContextAccessor
{
    /// <summary>
    /// Gets the current ambient security context, falling back to default when not set.
    /// </summary>
    IVKSecurityContext Current { get; }

    /// <summary>
    /// Begins an ambient security scope that restores previous security context upon disposal.
    /// </summary>
    /// <param name="context">The security context to apply.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous security context upon disposal.</returns>
    IDisposable BeginScope(IVKSecurityContext context);
}
