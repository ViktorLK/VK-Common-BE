using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Retrieval.Internal;

/// <summary>
/// Default gating policy that always triggers intent Cue extraction except for empty,
/// whitespace, or trivial single-punctuation/emoji inputs (Boundary Protection).
/// Ideal for single-user companion AI scenarios to maximize retrieval precision.
/// </summary>
internal sealed class AlwaysTriggerGatingPolicy : IVKPrefetchGatingPolicy
{
    public bool ShouldTriggerIntentExtraction(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        string trimmed = input.Trim();
        // Skip for trivial single-character punctuation/symbols/emojis
        if (trimmed.Length <= 1)
        {
            return false;
        }

        return true;
    }
}
