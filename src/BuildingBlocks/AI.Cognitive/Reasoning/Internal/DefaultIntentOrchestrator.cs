using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Reasoning.Internal;

/// <summary>
/// Default implementation of <see cref="IVKIntentNexus"/> that performs keyword heuristic intent classification.
/// </summary>
internal sealed class DefaultIntentOrchestrator : IVKIntentNexus
{
    private readonly IVKSemanticTextMatcher? _semanticMatcher;

    public DefaultIntentOrchestrator(IVKSemanticTextMatcher? semanticMatcher = null)
    {
        _semanticMatcher = semanticMatcher;
    }

    public async ValueTask<VKResult<VKIntentContext>> RouteAsync(string input, IVKAIArgs? args = null, CancellationToken ct = default)
    {
        VKGuard.NotNull(input);

        // [CS.01] Initialize deterministic values for fallback safety
        VKIntent bestIntent = VKIntent.Chat;
        double maxConfidence = 0.5; // Baseline

        if (_semanticMatcher is not null)
        {
            // Define intent anchor texts
            var anchors = new (VKIntent Intent, string Text)[]
            {
                (VKIntent.Roleplay, "Roleplay story character scenario immersive"),
                (VKIntent.Consulting, "Advice consulting help explain how to learn"),
                (VKIntent.Task, "Execute run plan do schedule job script"),
                (VKIntent.System, "System configuration admin toggle setting")
            };

            foreach (var anchor in anchors)
            {
                var simResult = await _semanticMatcher.CalculateSimilarityAsync(input, anchor.Text, ct).ConfigureAwait(false); // [CS.03]
                if (simResult.IsSuccess && simResult.Value > maxConfidence)
                {
                    maxConfidence = simResult.Value;
                    bestIntent = anchor.Intent;
                }
            }
        }
        else
        {
            // Fallback lightweight heuristic if semantic matcher is missing
            bool Contains(string word) => input.Contains(word, StringComparison.OrdinalIgnoreCase);

            if (Contains("rp") || Contains("story") || Contains("character") || Contains("roleplay"))
            {
                bestIntent = VKIntent.Roleplay;
                maxConfidence = 0.80;
            }
            else if (Contains("advisor") || Contains("help") || Contains("explain") || Contains("how to"))
            {
                bestIntent = VKIntent.Consulting;
                maxConfidence = 0.85;
            }
            else if (Contains("run") || Contains("do") || Contains("task") || Contains("execute") || Contains("plan"))
            {
                bestIntent = VKIntent.Task;
                maxConfidence = 0.90;
            }
            else if (Contains("system") || Contains("admin") || Contains("config") || Contains("setting"))
            {
                bestIntent = VKIntent.System;
                maxConfidence = 0.95;
            }
        }

        // [CS.01] Return non-null successful result carrying the evaluated intent context
        return VKResult.Success(new VKIntentContext
        {
            Intent = bestIntent,
            Confidence = maxConfidence,
            RefinedInput = input
        });
    }


}
