using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Common.DependencyInjection.Internal;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Pipeline;

/// <summary>
/// Symmetric middleware to handle user message prefixing before LLM call
/// and character prefix stripping after LLM call.
/// Follows AP.01 (sealed class default) and CS.03 (ConfigureAwait(false)).
/// </summary>
internal sealed class PwpRoleplayPrefixMiddleware : IVKPsycheMiddleware
{
    public int MiddlewareOrder => 90; // Runs before InvokeChatEngineAsync

    public async Task<VKResult> InvokeAsync(
        VKPsycheContext context,
        VKPipelineDelegate next,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);
        VKGuard.NotNull(next);
        cancellationToken.ThrowIfCancellationRequested();

        // 1. Resolve scoped PwpContext to get names
        var pwpContext = context.Services.GetRequiredService<PwpContext>();
        var userName = pwpContext.UserName;
        var charName = pwpContext.CharName;

        // 2. Invoke next in the middleware chain (which includes LLM call)
        var result = await next().ConfigureAwait(false); // [CS.03]
        if (result.IsFailure)
        {
            return result;
        }

        // 4. Strip character and roleplay prefixes from LLM response
        if (context.Response.ChatResponse is not null)
        {
            var rawResponse = context.Response.ChatResponse.Message.Content;
            var cleanedResponse = StripRoleplayPrefixes(rawResponse, userName, charName);

            context.Response.ChatResponse = context.Response.ChatResponse with
            {
                Message = context.Response.ChatResponse.Message with { Content = cleanedResponse }
            };
        }

        return result;
    }

    private static string StripRoleplayPrefixes(string text, string userName, string charName)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var prefixes = new[]
        {
            $"[{{char}}]:",
            $"[{{char}}]：",
            $"[{{personaname}}]:",
            $"[{{personaname}}]：",
            $"[{{character}}]:",
            $"[{{character}}]：",
            $"[{charName}]:",
            $"[{charName}]：",
            $"{{{{char}}}}:",
            $"{{{{char}}}}：",
            $"{{{{personaname}}}}:",
            $"{{{{personaname}}}}：",
            $"{{{{character}}}}:",
            $"{{{{character}}}}：",
            $"{charName}:",
            $"{charName}：",
            $"[{{user}}]:",
            $"[{{user}}]：",
            $"[{{username}}]:",
            $"[{{username}}]：",
            $"[{userName}]:",
            $"[{userName}]：",
            $"{{{{user}}}}:",
            $"{{{{user}}}}：",
            $"{{{{username}}}}:",
            $"{{{{username}}}}：",
            $"{userName}:",
            $"{userName}：",
            "assistant:",
            "assistant：",
            "user:",
            "user："
        };

        var trimmed = text.Trim();
        foreach (var prefix in prefixes)
        {
            if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed[prefix.Length..].Trim();
                return StripRoleplayPrefixes(trimmed, userName, charName);
            }
        }
        return trimmed;
    }
}
