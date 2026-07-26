using VK.Blocks.VectorIngest.DocumentLoader.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest;

/// <summary>
/// Service registration and options validation for DocumentLoader feature.
/// </summary>
[VKFeature(typeof(VKVectorIngestBlock), OptionsType = typeof(VKDocumentLoaderOptions))]
internal sealed partial class DocumentLoaderFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKDocumentLoaderOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKDocumentLoader, DefaultDocumentLoader>(); // [AP.02] TryAdd
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKDocumentLoaderOptions options, System.Collections.Generic.List<string> failures)
    {
        if (options.ChunkSize <= 0)
        {
            failures.Add("ChunkSize must be greater than 0.");
        }

        if (options.ChunkOverlap < 0)
        {
            failures.Add("ChunkOverlap must be greater than or equal to 0.");
        }

        if (options.ChunkOverlap >= options.ChunkSize)
        {
            failures.Add("ChunkOverlap must be less than ChunkSize.");
        }

        if (options.MaxDocumentSizeInBytes <= 0)
        {
            failures.Add("MaxDocumentSizeInBytes must be greater than 0.");
        }

        if (options.AllowedExtensions is null || options.AllowedExtensions.Length == 0)
        {
            failures.Add("AllowedExtensions cannot be null or empty.");
        }
    }
}
