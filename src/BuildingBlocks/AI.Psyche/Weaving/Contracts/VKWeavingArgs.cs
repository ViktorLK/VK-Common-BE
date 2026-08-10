using System.Collections.Generic;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Additional request-level arguments specific to prompt weaving execution.
/// Partial record extending the source-generated <see cref="VKWeavingArgs"/>.
/// </summary>
public partial record VKWeavingArgs
{
    /// <summary>
    /// Gets the key-value dictionary for template variable replacement across fragments in this request.
    /// </summary>
    public IDictionary<string, object?> Variables { get; init; } = new Dictionary<string, object?>();
}
