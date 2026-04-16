namespace VK.Blocks.Core.Diagnostics;

/// <summary>
/// Represents a security metadata entry for an endpoint.
/// </summary>
/// <param name="Key">The endpoint key (e.g., "Controller.Action").</param>
/// <param name="Metadata">The module-specific metadata object.</param>
/// <param name="Module">The name of the module providing this metadata (e.g., "Authentication").</param>
public sealed record SecurityMetadataEntry(string Key, object Metadata, string Module);
