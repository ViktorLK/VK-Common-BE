using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Revision.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Revision.Internal;

/// <summary>
/// Default implementation of <see cref="IVKContradictionArbitrator"/> using LLM chat engine.
/// </summary>
internal sealed class DefaultContradictionArbitrator : IVKContradictionArbitrator
{
    private readonly IVKChatEngine _chatEngine;
    private readonly VKConsolidationOptions? _options;
    private readonly ILogger<DefaultContradictionArbitrator> _logger;

    public DefaultContradictionArbitrator(
        IVKChatEngine chatEngine,
        ILogger<DefaultContradictionArbitrator> logger,
        IOptions<VKConsolidationOptions>? options = null)
    {
        _chatEngine = VKGuard.NotNull(chatEngine);
        _logger = VKGuard.NotNull(logger);
        _options = options?.Value;
    }

    public async Task<VKResult<VKContradictionArbitrationResult>> ArbitrateAsync(
        string newFact,
        IReadOnlyList<VKMemoryEntry> existingCandidates,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(newFact);
        VKGuard.NotNull(existingCandidates);

        if (existingCandidates.Count == 0)
        {
            return VKResult.Success(new VKContradictionArbitrationResult
            {
                Kind = VKContradictionKind.None
            });
        }

        var candidatesText = string.Join("\n", existingCandidates.Select((c, idx) => $"[{idx}] ID:{c.Id} => {c.Content}"));

        string prompt = "Analyze if the NEW FACT contradicts or updates any of the EXISTING MEMORIES.\n\n" +
                        $"NEW FACT:\n{newFact}\n\n" +
                        $"EXISTING MEMORIES:\n{candidatesText}\n\n" +
                        "Determine if there is a conflict or update. Output exactly on the first line:\n" +
                        "- EXPLICIT_CORRECTION:<ID> (if new fact invalidates an existing memory by ID)\n" +
                        "- SEMANTIC_DRIFT:<ID> (if new fact naturally evolves an existing memory)\n" +
                        "- UNRESOLVED_CONTRADICTION:<ID> (if conflicting without clear resolution)\n" +
                        "- NONE (if no conflict/update)\n\n" +
                        "If EXPLICIT_CORRECTION or SEMANTIC_DRIFT, output the updated refined fact text on the next line.";

        var messages = new[] { VKChatMessage.FromText(VKChatRole.User, prompt) };

        VKChatArgs? chatArgs = null;
        string? targetModel = _options?.ArbitrationModelId ?? _options?.ModelId;
        if (!string.IsNullOrWhiteSpace(targetModel))
        {
            chatArgs = new VKChatArgs { ModelId = targetModel };
        }

        try
        {
            var result = await _chatEngine.SendAsync(messages, chatArgs, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure || string.IsNullOrWhiteSpace(result.Value.Message.Content))
            {
                return VKResult.Success(new VKContradictionArbitrationResult { Kind = VKContradictionKind.None });
            }

            var lines = result.Value.Message.Content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            string header = lines[0].Trim();

            if (header.Equals("NONE", StringComparison.OrdinalIgnoreCase))
            {
                return VKResult.Success(new VKContradictionArbitrationResult { Kind = VKContradictionKind.None });
            }

            var parts = header.Split(':', 2);
            string code = parts[0].ToUpperInvariant();
            string? targetId = parts.Length > 1 ? parts[1].Trim() : null;
            string? refinedFact = lines.Length > 1 ? string.Join("\n", lines.Skip(1)).Trim() : newFact;

            VKContradictionKind kind = code switch
            {
                "EXPLICIT_CORRECTION" => VKContradictionKind.ExplicitCorrection,
                "SEMANTIC_DRIFT" => VKContradictionKind.SemanticDrift,
                "UNRESOLVED_CONTRADICTION" => VKContradictionKind.UnresolvedContradiction,
                _ => VKContradictionKind.None
            };

            _logger.RevisionArbitrationCompleted(kind, targetId);

            return VKResult.Success(new VKContradictionArbitrationResult
            {
                Kind = kind,
                ContradictedMemoryId = targetId,
                RefinedFact = refinedFact
            });
        }
        catch (Exception ex)
        {
            _logger.RevisionArbitrationError(ex);
            return VKResult.Success(new VKContradictionArbitrationResult { Kind = VKContradictionKind.None });
        }
    }
}
