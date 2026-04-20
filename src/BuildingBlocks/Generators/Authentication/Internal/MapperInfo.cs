using Microsoft.CodeAnalysis;

namespace VK.Blocks.Generators.Authentication.Internal;

/// <summary>
/// Intermediate model for OAuth mapper information used during the generation process.
/// </summary>
/// <param name="ProviderName">The unique name of the OAuth provider.</param>
/// <param name="FullClassName">The full type name of the mapper class.</param>
/// <param name="Location">The source code location of the mapper declaration.</param>
internal sealed record MapperInfo(string ProviderName, string FullClassName, Location Location);
