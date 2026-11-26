namespace VK.Blocks.AI;

/// <summary>
/// Exposes an untyped materialized structured output model without runtime reflection.
/// </summary>
// [ARCH-02] [AP.03]
public interface IVKStructuredModelEnvelope
{
    /// <summary>
    /// Gets the underlying materialized model instance, if available.
    /// </summary>
    object? RawModel { get; }
}
