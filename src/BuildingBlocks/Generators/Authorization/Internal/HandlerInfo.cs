namespace VK.Blocks.Generators.Authorization.Internal;

/// <summary>
/// Intermediate model for authorization handler information used during the generation process.
/// </summary>
/// <param name="ImplementationName">The full type name of the handler implementation.</param>
/// <param name="IsPermissionEvaluator">Whether the handler also implements IPermissionEvaluator.</param>
internal sealed record HandlerInfo(string ImplementationName, bool IsPermissionEvaluator);
