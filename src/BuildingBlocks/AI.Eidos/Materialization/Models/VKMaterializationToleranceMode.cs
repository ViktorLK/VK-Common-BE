namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Defines tolerance level for contract parsing and schema validation failures.
/// </summary>
public enum VKMaterializationToleranceMode : byte
{
    /// <summary>
    /// Strict mode (default). Any validation violation or deserialization failure aborts binding.
    /// </summary>
    Strict = 0,

    /// <summary>
    /// Lenient mode. Allows missing optional fields and applies default values with warnings.
    /// </summary>
    Lenient = 1,

    /// <summary>
    /// Partial mode. Accepts partially valid collections/arrays by discarding invalid elements.
    /// </summary>
    Partial = 2
}
