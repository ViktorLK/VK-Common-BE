using VK.Blocks.VectorIngest.Parsing.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest; // [AP.03] Internal namespace

/// <summary>
/// Configures and registers dependencies for the Parsing feature.
/// </summary>
[VKFeature(typeof(VKVectorIngestBlock), OptionsType = typeof(VKParsingOptions))]
internal sealed partial class ParsingFeature // [AP.01] sealed partial
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKParsingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKDocumentParserResolver, DefaultDocumentParserResolver>(); // [AP.02] TryAdd idempotent registration
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKParsingOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
