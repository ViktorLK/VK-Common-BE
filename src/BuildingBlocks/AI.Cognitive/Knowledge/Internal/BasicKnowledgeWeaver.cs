using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VK.Blocks.AI.Cognitive;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Knowledge.Internal;

/// <summary>
/// High-performance implementation of <see cref="IVKKnowledgeWeaver"/> supporting SillyTavern prompt layouts.
/// </summary>
// // [AP.01] Sealed default for classes
internal sealed class BasicKnowledgeWeaver : IVKKnowledgeWeaver
{
    public VKResult<IEnumerable<VKChatMessage>> Weave(
        IEnumerable<VKChatMessage> messages,
        string? systemInstructions,
        IEnumerable<VKKnowledgeEntry> retrievedEntries)
    {
        // // [AP.01] Boundary checks using VKGuard
        VKGuard.NotNull(messages);
        VKGuard.NotNull(retrievedEntries);

        var entries = retrievedEntries.ToList();
        if (entries.Count == 0)
        {
            // Direct pass-through if no knowledge entries are triggered
            var baseList = new List<VKChatMessage>();
            if (!string.IsNullOrWhiteSpace(systemInstructions))
            {
                baseList.Add(new VKChatMessage { Role = VKChatRole.System, Content = systemInstructions });
            }
            baseList.AddRange(messages);
            return VKResult.Success<IEnumerable<VKChatMessage>>(baseList);
        }

        var finalMessages = new List<VKChatMessage>();

        // 1. Build Woven System Instructions (Defs Level)
        var systemInstructionsBuilder = new StringBuilder();
        
        // BeforeDefs
        var beforeDefs = entries.Where(e => e.Weaving.Position == VKKnowledgePositions.BeforeDefs)
                               .OrderBy(e => e.Weaving.Priority)
                               .ThenBy(e => e.Weaving.Weight);
        foreach (var entry in beforeDefs)
        {
            if (systemInstructionsBuilder.Length > 0)
            {
                systemInstructionsBuilder.AppendLine();
            }
            systemInstructionsBuilder.Append(entry.Content);
        }

        // Base instructions
        if (!string.IsNullOrWhiteSpace(systemInstructions))
        {
            if (systemInstructionsBuilder.Length > 0)
            {
                systemInstructionsBuilder.AppendLine();
            }
            systemInstructionsBuilder.Append(systemInstructions);
        }

        // AfterDefs
        var afterDefs = entries.Where(e => e.Weaving.Position == VKKnowledgePositions.AfterDefs)
                              .OrderBy(e => e.Weaving.Priority)
                              .ThenBy(e => e.Weaving.Weight);
        foreach (var entry in afterDefs)
        {
            if (systemInstructionsBuilder.Length > 0)
            {
                systemInstructionsBuilder.AppendLine();
            }
            systemInstructionsBuilder.Append(entry.Content);
        }

        var finalSystemText = systemInstructionsBuilder.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(finalSystemText))
        {
            finalMessages.Add(new VKChatMessage { Role = VKChatRole.System, Content = finalSystemText });
        }

        // Add base conversation messages
        var historyList = messages.ToList();
        finalMessages.AddRange(historyList);

        // 2. Weave Message-Level depth injections (SystemAtDepth, UserAtDepth, AssistantAtDepth)
        var depthEntries = entries
            .Where(e => e.Weaving.Position == VKKnowledgePositions.SystemAtDepth ||
                        e.Weaving.Position == VKKnowledgePositions.UserAtDepth ||
                        e.Weaving.Position == VKKnowledgePositions.AssistantAtDepth)
            .OrderBy(e => e.Weaving.Priority)
            .ThenBy(e => e.Weaving.Weight);

        foreach (var entry in depthEntries)
        {
            var targetRole = entry.Weaving.Position switch
            {
                VKKnowledgePositions.SystemAtDepth => VKChatRole.System,
                VKKnowledgePositions.UserAtDepth => VKChatRole.User,
                VKKnowledgePositions.AssistantAtDepth => VKChatRole.Assistant,
                _ => VKChatRole.System
            };

            // Calculate target message index based on depth (reverse search from end of historyList)
            int currentDepth = 0;
            int insertIndex = -1;

            // Search back from last message
            for (int i = finalMessages.Count - 1; i >= 0; i--)
            {
                if (finalMessages[i].Role == targetRole)
                {
                    if (currentDepth == entry.Weaving.Depth)
                    {
                        insertIndex = i;
                        break;
                    }
                    currentDepth++;
                }
            }

            if (insertIndex >= 0)
            {
                // Inject at depth
                finalMessages.Insert(insertIndex, new VKChatMessage 
                { 
                    Role = targetRole, 
                    Content = entry.Content
                });
            }
            else
            {
                // Fallback: prepend to history (above active conversation, after system instructions)
                int systemCount = finalMessages.TakeWhile(m => m.Role == VKChatRole.System).Count();
                finalMessages.Insert(systemCount, new VKChatMessage { Role = targetRole, Content = entry.Content });
            }
        }

        // 3. Weave Author's Note Level (BeforeAuthorNote / AfterAuthorNote)
        var authorNoteEntries = entries
            .Where(e => e.Weaving.Position == VKKnowledgePositions.BeforeAuthorNote ||
                        e.Weaving.Position == VKKnowledgePositions.AfterAuthorNote)
            .OrderBy(e => e.Weaving.Priority)
            .ThenBy(e => e.Weaving.Weight);

        foreach (var entry in authorNoteEntries)
        {
            // Author's Note is standard System Message injected at turn depth (typically depth = 1 or 2)
            int targetDepth = entry.Weaving.Depth > 0 ? entry.Weaving.Depth : 1;
            int insertIndex = Math.Max(0, finalMessages.Count - targetDepth);

            if (entry.Weaving.Position == VKKnowledgePositions.AfterAuthorNote)
            {
                insertIndex = Math.Min(finalMessages.Count, insertIndex + 1);
            }

            finalMessages.Insert(insertIndex, new VKChatMessage 
            { 
                Role = VKChatRole.System, 
                Content = entry.Content 
            });
        }

        // 4. Weave Example Message Level (BeforeExampleMessages / AfterExampleMessages)
        var exampleEntries = entries
            .Where(e => e.Weaving.Position == VKKnowledgePositions.BeforeExampleMessages ||
                        e.Weaving.Position == VKKnowledgePositions.AfterExampleMessages)
            .OrderBy(e => e.Weaving.Priority)
            .ThenBy(e => e.Weaving.Weight);

        foreach (var entry in exampleEntries)
        {
            // Search for example messages (often recognized by '<START>' boundary marker or specific metadata)
            int exampleIndex = -1;
            for (int i = 0; i < finalMessages.Count; i++)
            {
                if (finalMessages[i].Content.Contains("<START>") || 
                    finalMessages[i].Metadata?.ContainsKey("IsExample") == true)
                {
                    exampleIndex = i;
                    break;
                }
            }

            if (exampleIndex >= 0)
            {
                int insertIndex = entry.Weaving.Position == VKKnowledgePositions.BeforeExampleMessages
                    ? exampleIndex
                    : exampleIndex + 1;
                
                finalMessages.Insert(insertIndex, new VKChatMessage 
                { 
                    Role = VKChatRole.System, 
                    Content = entry.Content 
                });
            }
            else
            {
                // Fallback: prepend to history (above active conversation, after system instructions)
                int systemCount = finalMessages.TakeWhile(m => m.Role == VKChatRole.System).Count();
                finalMessages.Insert(systemCount, new VKChatMessage { Role = VKChatRole.System, Content = entry.Content });
            }
        }

        return VKResult.Success<IEnumerable<VKChatMessage>>(finalMessages);
    }
}
