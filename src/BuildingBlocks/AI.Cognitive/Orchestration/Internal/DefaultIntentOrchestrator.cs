using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Orchestration.Internal;

/// <summary>
/// Default implementation of <see cref="IVKIntentNexus"/> that performs keyword heuristic intent classification.
/// </summary>
internal sealed class DefaultIntentOrchestrator : IVKIntentNexus
{
    public ValueTask<VKResult<VKIntentContext>> RouteAsync(string input, IVKAIArgs? args = null, CancellationToken ct = default)
    {
        VKGuard.NotNull(input);

        // // [CS.01] Initialize deterministic values for fallback safety
        VKIntent intent = VKIntent.Chat;
        double confidence = 1.0;

        if (input.Contains("rp", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("story", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("character", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("roleplay", StringComparison.OrdinalIgnoreCase))
        {
            intent = VKIntent.Roleplay;
            confidence = 0.80;
        }
        else if (input.Contains("advisor", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("help", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("advice", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("consult", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("explain", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("how to", StringComparison.OrdinalIgnoreCase))
        {
            intent = VKIntent.Consulting;
            confidence = 0.85;
        }
        else if (input.Contains("run", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("do", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("task", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("job", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("schedule", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("execute", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("plan", StringComparison.OrdinalIgnoreCase))
        {
            intent = VKIntent.Task;
            confidence = 0.90;
        }
        else if (input.Contains("system", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("admin", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("config", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("setting", StringComparison.OrdinalIgnoreCase) ||
                 input.Contains("toggle", StringComparison.OrdinalIgnoreCase))
        {
            intent = VKIntent.System;
            confidence = 0.95;
        }

        // // [CS.01] Return non-null successful result carrying the evaluated intent context
        return ValueTask.FromResult(VKResult.Success(new VKIntentContext
        {
            Intent = intent,
            Confidence = confidence,
            RefinedInput = input
        }));
    }
}
