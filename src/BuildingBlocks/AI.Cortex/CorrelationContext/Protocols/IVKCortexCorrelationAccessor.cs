using System;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Accessor interface to retrieve and scope ambient <see cref="VKCortexCorrelationContext"/> across asynchronous execution scopes.
/// Follows [AP.01] and [CS.01].
/// </summary>
public interface IVKCortexCorrelationAccessor
{
    /// <summary>
    /// Gets the current ambient correlation context.
    /// </summary>
    VKCortexCorrelationContext? CurrentContext { get; }

    /// <summary>
    /// Begins a scoped execution block with the specified correlation context.
    /// Restores the previous context upon disposal to prevent execution context leakage.
    /// </summary>
    /// <param name="context">The correlation context for the scope.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous context when disposed.</returns>
    IDisposable BeginScope(VKCortexCorrelationContext context);
}
