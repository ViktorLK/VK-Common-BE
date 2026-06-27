using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Parsing.Internal; // [AP.03] Internal namespace

/// <summary>
/// Configures and registers dependencies for the Parsing feature.
/// </summary>
internal sealed partial class ParsingFeature // [AP.01] sealed partial
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKParsingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKDocumentParserResolver, DefaultDocumentParserResolver>(); // [AP.02] TryAdd idempotent registration
    }

    // [SG Hook]
    static partial void ValidateCustom(VKParsingOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
