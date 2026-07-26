using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Engram.Consolidation.Diagnostics.Internal;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch;
using VK.Blocks.VectorStore;

namespace VK.Blocks.AI.Engram.Consolidation.Internal;

internal sealed class DefaultConsolidationPersistenceManager : IVKConsolidationPersistenceManager
{
    private readonly IVKMemoryStore _store;
    private readonly IVKConsolidationDlqHandler? _dlqHandler;
    private readonly IVKRetrievalStore? _retrievalStore;
    private readonly IVKEmbeddingsEngine? _embeddingsEngine;
    private readonly ILogger<DefaultConsolidationPersistenceManager> _logger;

    public DefaultConsolidationPersistenceManager(
        IVKMemoryStore store,
        ILogger<DefaultConsolidationPersistenceManager> logger,
        IVKConsolidationDlqHandler? dlqHandler = null,
        IVKRetrievalStore? retrievalStore = null,
        IVKEmbeddingsEngine? embeddingsEngine = null)
    {
        _store = VKGuard.NotNull(store);
        _logger = VKGuard.NotNull(logger);
        _dlqHandler = dlqHandler;
        _retrievalStore = retrievalStore;
        _embeddingsEngine = embeddingsEngine;
    }

    public async Task<VKResult> PersistEntriesAsync(
        IReadOnlyList<VKMemoryEntry> entries,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entries);

        foreach (var entry in entries)
        {
            int attempts = 0;
            bool saved = false;
            while (attempts < 3 && !saved)
            {
                attempts++;
                var saveResult = await _store.UpsertAsync(entry, cancellationToken).ConfigureAwait(false);
                if (saveResult.IsSuccess)
                {
                    saved = true;

                    if (_retrievalStore is not null)
                    {
                        var chunk = new VKDocumentChunk
                        {
                            Id = entry.Id.ToString(),
                            DocumentId = entry.TenantId?.Value.ToString() ?? "default",
                            Content = entry.Content,
                            Metadata = new VKVectorMetadata { TenantId = entry.TenantId?.Value.ToString() ?? "default" }
                        };

                        VKVector embedding;
                        if (_embeddingsEngine is not null)
                        {
                            var embResult = await _embeddingsEngine.GenerateAsync(entry.Content, cancellationToken).ConfigureAwait(false);
                            embedding = embResult.IsSuccess ? embResult.Value : new VKVector { Values = new float[384] };
                        }
                        else
                        {
                            embedding = new VKVector { Values = new float[384] };
                        }

                        await _retrievalStore.UpsertAsync([chunk], [embedding], cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (attempts < 3)
                {
                    _logger.PersistenceRetry(attempts, entry.Id.ToString());
                    await Task.Delay(100 * attempts, cancellationToken).ConfigureAwait(false);
                }
            }

            if (!saved)
            {
                _logger.PersistenceFailedDlq(entry.Id.ToString(), entry.Content.Length);

                if (_dlqHandler is not null)
                {
                    await _dlqHandler.HandleFailedEntryAsync(entry, null, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        return VKResult.Success();
    }
}
