namespace VK.Blocks.Generators.Authorization.Internal;

/// <summary>
/// Intermediate model for permission provider information used during the generation process.
/// </summary>
/// <param name="ModuleName">The name of the module.</param>
/// <param name="Source">The source of the permissions (e.g., Claims, Database).</param>
/// <param name="TargetTypeFullName">The full type name of the target being handled.</param>
internal sealed record ProviderInfo(string ModuleName, string Source, string TargetTypeFullName);
