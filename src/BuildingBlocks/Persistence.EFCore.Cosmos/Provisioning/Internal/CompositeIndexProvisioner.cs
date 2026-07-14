using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Cosmos.Common.Diagnostics.Internal;

namespace VK.Blocks.Persistence.EFCore.Cosmos.Provisioning.Internal;

/// <summary>
/// Automatically provisions composite indexes necessary for complex ORDER BY scenarios.
/// </summary>
internal sealed class CompositeIndexProvisioner
{
    private readonly ILogger<CompositeIndexProvisioner> _logger;

    public CompositeIndexProvisioner(ILogger<CompositeIndexProvisioner> logger)
    {
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ProvisionCompositeIndexesAsync(
        Container container,
        IReadOnlyList<VKCompositeIndexDefinition> definitions,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(container);
        VKGuard.NotNull(definitions);

        if (definitions.Count == 0)
        {
            return VKResult.Success();
        }

        try
        {
            var response = await container.ReadContainerAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var properties = response.Resource;

            bool modified = false;
            foreach (var definition in definitions)
            {
                var compositePathList = new System.Collections.ObjectModel.Collection<CompositePath>();
                foreach (var field in definition.Fields)
                {
                    compositePathList.Add(new CompositePath
                    {
                        Path = field.Path,
                        Order = field.Descending ? CompositePathSortOrder.Descending : CompositePathSortOrder.Ascending
                    });
                }

                // Check if already exists to prevent duplication
                bool exists = false;
                foreach (var existing in properties.IndexingPolicy.CompositeIndexes)
                {
                    if (IsMatch(existing, compositePathList))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    properties.IndexingPolicy.CompositeIndexes.Add(compositePathList);
                    modified = true;
                }
            }

            if (modified)
            {
                await container.ReplaceContainerAsync(properties, cancellationToken: cancellationToken).ConfigureAwait(false);
                CosmosLog.LogCompositeIndexProvisioned(_logger, container.Id, definitions.Count);
            }

            return VKResult.Success();
        }
        catch (Exception ex)
        {
            return VKResult.Failure(VKError.Failure("Persistence.Cosmos.Indexing.ProvisionFailed", ex.Message));
        }
    }

    private static bool IsMatch(
        System.Collections.ObjectModel.Collection<CompositePath> a,
        System.Collections.ObjectModel.Collection<CompositePath> b)
    {
        if (a.Count != b.Count)
            return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i].Path != b[i].Path || a[i].Order != b[i].Order)
                return false;
        }
        return true;
    }
}
