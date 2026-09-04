namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Compatibility classification level between two schema versions.
/// </summary>
public enum VKSchemaCompatibilityLevel : byte
{
    /// <summary>
    /// Schemas are structurally identical.
    /// </summary>
    Identical = 0,

    /// <summary>
    /// Target schema is backward-compatible with source schema (e.g. optional fields added).
    /// </summary>
    Compatible = 1,

    /// <summary>
    /// Target schema contains breaking changes (e.g. fields removed, required fields added, types changed).
    /// </summary>
    Breaking = 2
}
