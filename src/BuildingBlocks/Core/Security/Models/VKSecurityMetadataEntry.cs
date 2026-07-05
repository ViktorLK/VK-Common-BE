namespace VK.Blocks.Core;

/// <summary>
/// Represents a security metadata entry for an endpoint.
/// </summary>
/// <param name="Key">The endpoint key (e.g., "Controller.Action").</param>
/// <param name="Metadata">The module-specific metadata object.</param>
/// <param name="Module">The name of the module providing this metadata (e.g., "Authentication").</param>
public sealed record VKSecurityMetadataEntry(string Key, object Metadata, string Module);
