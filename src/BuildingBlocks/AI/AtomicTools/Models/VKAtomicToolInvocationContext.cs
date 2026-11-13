using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Context passed through the <see cref="IVKAtomicToolFilter"/> onion pipeline.
/// </summary>
public sealed record VKAtomicToolInvocationContext(
    IVKAtomicTool Tool,
    IDictionary<string, object> Arguments,
    VKAtomicToolContext Context);

