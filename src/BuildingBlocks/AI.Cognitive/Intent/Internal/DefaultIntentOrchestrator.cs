using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Intent.Internal;

/// <summary>
/// Default implementation of <see cref="IVKIntentRouter"/> that performs keyword heuristic intent classification.
/// </summary>
internal sealed class DefaultIntentOrchestrator : IVKIntentRouter
{
    private readonly IVKSemanticTextMatcher? _semanticMatcher;

    public DefaultIntentOrchestrator(IVKSemanticTextMatcher? semanticMatcher = null)
    {
        _semanticMatcher = semanticMatcher;
    }

    public async ValueTask<VKResult<VKIntentContext>> RouteAsync(string input, IVKAIArgs? args = null, CancellationToken ct = default)
    {
        VKGuard.NotNull(input); // [AP.01]

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
                var simScore = await _semanticMatcher.ComputeSimilarityAsync(input, anchor.Text, ct).ConfigureAwait(false); // [CS.03]
                if (simScore > maxConfidence)
                {
                    maxConfidence = simScore;
                    bestIntent = anchor.Intent;
                }
            }
        }
        else
        {
            // Fallback lightweight heuristic if semantic matcher is missing
            static bool Contains(string text, string word) => text.Contains(word, StringComparison.OrdinalIgnoreCase);

            if (Contains(input, "rp") || Contains(input, "story") || Contains(input, "character") || Contains(input, "roleplay"))
            {
                bestIntent = VKIntent.Roleplay;
                maxConfidence = 0.80;
            }
            else if (Contains(input, "advisor") || Contains(input, "help") || Contains(input, "explain") || Contains(input, "how to"))
            {
                bestIntent = VKIntent.Consulting;
                maxConfidence = 0.85;
            }
            else if (Contains(input, "run") || Contains(input, "do") || Contains(input, "task") || Contains(input, "execute") || Contains(input, "plan"))
            {
                bestIntent = VKIntent.Task;
                maxConfidence = 0.90;
            }
            else if (Contains(input, "system") || Contains(input, "admin") || Contains(input, "config") || Contains(input, "setting"))
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
            RefinedInput = input,
            Source = "Heuristic"
        });
    }
}
