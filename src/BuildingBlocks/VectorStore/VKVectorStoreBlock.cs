using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Architectural marker for the AI Vector Store building block.
/// Enforces fail-fast startup validation for logical collections, dimensions, and model IDs [BASE-03].
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)], Toggleable = false)]
public sealed partial class VKVectorStoreBlock
{

    static partial void RegisterBlockCustom(IVKVectorStoreBuilder builder)
    {
    }

    static partial void ValidateBlockCustom(VKVectorStoreOptions options, List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);

        if (string.IsNullOrWhiteSpace(options.DefaultCollection))
        {
            failures.Add("VectorStoreOptions: DefaultCollection cannot be empty.");
        }

        if (options.DefaultLimit <= 0)
        {
            failures.Add("VectorStoreOptions: DefaultLimit must be greater than zero.");
        }

        if (options.DefaultMinScore < 0.0f || options.DefaultMinScore > 1.0f)
        {
            failures.Add("VectorStoreOptions: DefaultMinScore must be between 0.0 and 1.0.");
        }

        // Validate declared Logical Collections
        foreach (var (logicalName, def) in options.LogicalCollections)
        {
            if (string.IsNullOrWhiteSpace(logicalName))
            {
                failures.Add("VectorStoreOptions: LogicalCollection key cannot be empty.");
                continue;
            }

            if (def is null)
            {
                failures.Add($"VectorStoreOptions: LogicalCollection '{logicalName}' definition cannot be null.");
                continue;
            }

            if (def.ExpectedDimensions.HasValue && def.ExpectedDimensions.Value <= 0)
            {
                failures.Add($"VectorStoreOptions: LogicalCollection '{logicalName}' ExpectedDimensions must be greater than zero if specified.");
            }
        }
    }

}
