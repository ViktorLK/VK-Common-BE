using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Directive.Internal;

/// <summary>
/// Default implementation of <see cref="IVKDirectiveRenderer"/>.
/// Follows AP.01 (sealed class).
/// </summary>
internal sealed class DefaultDirectiveRenderer : IVKDirectiveRenderer
{
    public string Render(VKDirectiveCharter directive)
    {
        VKGuard.NotNull(directive); // [AP.01]

        var items = new List<string>(4);
        if (!string.IsNullOrWhiteSpace(directive.BehaviorRules))
        {
            items.Add(directive.BehaviorRules);
        }

        if (!string.IsNullOrWhiteSpace(directive.SafetyRules))
        {
            items.Add(directive.SafetyRules);
        }

        if (!string.IsNullOrWhiteSpace(directive.OutputConstraints))
        {
            items.Add(directive.OutputConstraints);
        }

        if (!string.IsNullOrWhiteSpace(directive.Overview))
        {
            items.Add(directive.Overview);
        }

        return items.Count > 0 ? string.Join(Environment.NewLine, items) : string.Empty;
    }
}
