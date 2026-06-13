using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Vectorics.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Vectorics.Retrieval.Internal;

/// <summary>
/// Default implementation of <see cref="IVKAIRagOrchestrator"/>.
/// </summary>
internal sealed class VKAIRagOrchestrator : IVKAIRagOrchestrator
{
    private readonly IVKRetrievalEngine _retrievalEngine;
    private readonly IVKChatEngine _chatEngine;
    private readonly ILogger<VKAIRagOrchestrator> _logger;

    public VKAIRagOrchestrator(
        IVKRetrievalEngine retrievalEngine,
        IVKChatEngine chatEngine,
        ILogger<VKAIRagOrchestrator> logger)
    {
        _retrievalEngine = VKGuard.NotNull(retrievalEngine);
        _chatEngine = VKGuard.NotNull(chatEngine);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKChatResponse>> GenerateAsync(
        string query,
        VKRetrievalArgs? retrievalArgs = null,
        IVKAIArgs? chatArgs = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(query);

        // 1. Retrieve Context
        var retrievalResult = await _retrievalEngine.SearchAsync(query, retrievalArgs, cancellationToken).ConfigureAwait(false);
        if (retrievalResult.IsFailure)
        {
            VectoricsDiagnostics.RetrievalFailed(_logger);
        }

        // 2. Build Messages
        var messages = BuildMessages(query, retrievalResult.IsSuccess ? retrievalResult.Value : []);

        // 3. Generate Response
        return await _chatEngine.SendAsync(messages, chatArgs, cancellationToken).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<VKResult<VKChatStreamingResponse>> GenerateStreamingAsync(
        string query,
        VKRetrievalArgs? retrievalArgs = null,
        IVKAIArgs? chatArgs = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(query);

        // 1. Retrieve Context
        var retrievalResult = await _retrievalEngine.SearchAsync(query, retrievalArgs, cancellationToken).ConfigureAwait(false);
        if (retrievalResult.IsFailure)
        {
            VectoricsDiagnostics.RetrievalFailed(_logger);
        }

        // 2. Build Messages
        var messages = BuildMessages(query, retrievalResult.IsSuccess ? retrievalResult.Value : []);

        // 3. Stream Response
        await foreach (var chunk in _chatEngine.SendStreamingAsync(messages, chatArgs, cancellationToken).ConfigureAwait(false))
        {
            yield return chunk;
        }
    }

    private static List<VKChatMessage> BuildMessages(string query, IReadOnlyList<VKRetrievalResult> contextResults)
    {
        var messages = new List<VKChatMessage>();

        if (contextResults.Count > 0)
        {
            var contextPart = new VKRetrievalContextPart { Results = contextResults };
            var sysMessage = new VKChatMessage
            {
                Role = VKChatRole.System,
                Parts = [contextPart]
            };
            messages.Add(sysMessage);
        }

        messages.Add(VKChatMessage.FromText(VKChatRole.User, query));

        return messages;
    }
}
