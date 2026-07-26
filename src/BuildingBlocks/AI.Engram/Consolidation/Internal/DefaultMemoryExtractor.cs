using System;
using System.Collections.Generic;
using System.Linq;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Engram.Consolidation.Internal;

internal sealed class DefaultMemoryExtractor : IVKMemoryExtractor
{
    public bool TryExtract(VKPsycheContext context, out string[] memoriesToSave)
    {
        memoriesToSave = Array.Empty<string>();
        if (context.Response.ChatResponse?.Message?.Content is null)
        {
            return false;
        }

        var list = new List<string>();

        // Find the last user message
        var lastUserMsg = context.Response.Messages
            .LastOrDefault(m => m.Role == VKChatRole.User);

        if (lastUserMsg?.Content is not null)
        {
            list.Add($"User: {lastUserMsg.Content}");
        }

        list.Add($"Assistant: {context.Response.ChatResponse.Message.Content}");

        memoriesToSave = [.. list];
        return memoriesToSave.Length > 0;
    }
}
