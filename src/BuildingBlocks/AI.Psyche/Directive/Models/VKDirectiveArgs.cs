namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Runtime overrides or arguments to adjust the Directive parsing in this turn.
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKDirectiveArgs
{
    /// <summary>
    /// Gets the unique identifier of the Directive Charter to load. 
    /// Overrides the Persona or Tenant default if specified.
    /// </summary>
    public string? DirectiveId { get; init; }
}
