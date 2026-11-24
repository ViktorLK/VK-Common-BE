using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.AI;
using VK.Blocks.AI.MEAI.AtomicTools.Internal;

namespace VK.Blocks.AI.MEAI.Chat.Internal;

/// <summary>
/// Translates VK chat options and per-call arguments into MEAI <see cref="ChatOptions"/>,
/// including schema caching, tool adaptation, and provider model/deployment mapping.
/// </summary>
internal static class ChatOptionsTranslator
{
    private static readonly ConcurrentDictionary<string, JsonElement> SchemaCache = new(StringComparer.Ordinal);

    public static ChatOptions? Translate(
        VKChatArgs? args,
        VKChatOptions? defaultChat,
        VKAIMEAIOptions? extensionsOptions,
        IEnumerable<IVKAtomicTool> registeredTools,
        IEnumerable<IVKAtomicToolFilter> toolFilters)
    {
        if (args is null && defaultChat is null)
        {
            return null;
        }

        var rawModelId = args?.ModelId ?? defaultChat?.ModelId;
        var provider = args?.Provider ?? defaultChat?.Provider;
        var modelId = ResolveDeploymentIfMapped(rawModelId, provider, extensionsOptions);

        var options = new ChatOptions
        {
            Temperature = args?.Temperature ?? defaultChat?.Temperature,
            MaxOutputTokens = args?.MaxTokens ?? defaultChat?.MaxTokens,
            TopP = args?.TopP ?? defaultChat?.TopP,
            TopK = args?.TopK ?? defaultChat?.TopK,
            FrequencyPenalty = args?.FrequencyPenalty ?? defaultChat?.FrequencyPenalty,
            PresencePenalty = args?.PresencePenalty ?? defaultChat?.PresencePenalty,
            ModelId = modelId
        };

        var stopSequences = (args?.StopSequences is not null && args.StopSequences.Count > 0)
            ? args.StopSequences
            : defaultChat?.StopSequences;

        if (stopSequences is not null && stopSequences.Count > 0)
        {
            options.StopSequences = stopSequences.ToList();
        }

        var enabledTools = extensionsOptions?.ChainSettings.EnabledTools;
        var explicitTools = (args?.Tools is { Count: > 0 })
            ? args.Tools
            : defaultChat?.Tools;

        var toolsToInclude = AtomicToolAdapter.AdaptTools(
            explicitTools,
            registeredTools,
            enabledTools,
            filters: toolFilters);

        if (toolsToInclude.Count > 0)
        {
            options.Tools = toolsToInclude;
        }

        var autoInvokeTools = args?.AutoInvokeTools ?? defaultChat?.AutoInvokeTools;
        if (autoInvokeTools.HasValue)
        {
            options.AdditionalProperties ??= new AdditionalPropertiesDictionary();
            options.AdditionalProperties["vk.autoInvokeTools"] = autoInvokeTools.Value;
        }

        if (defaultChat is not null)
        {
            options.AdditionalProperties ??= new AdditionalPropertiesDictionary();
            options.AdditionalProperties["vk.maxAutoToolRounds"] = defaultChat.MaxAutoToolRounds;
        }

        var toolChoice = args?.ToolChoice ?? defaultChat?.ToolChoice;
        if (!string.IsNullOrWhiteSpace(toolChoice))
        {
            options.ToolMode = toolChoice.ToUpperInvariant() switch
            {
                "AUTO" => ChatToolMode.Auto,
                "NONE" => ChatToolMode.None,
                "REQUIRED" => ChatToolMode.RequireAny,
                _ => ChatToolMode.RequireSpecific(toolChoice)
            };
        }

        if (!string.IsNullOrWhiteSpace(args?.ResponseSchema))
        {
            var cachedSchema = SchemaCache.GetOrAdd(args.ResponseSchema, static schemaText =>
            {
                using var doc = JsonDocument.Parse(schemaText);
                return doc.RootElement.Clone();
            });
            options.ResponseFormat = ChatResponseFormat.ForJsonSchema(cachedSchema);
        }

        if (!string.IsNullOrWhiteSpace(args?.TurnId))
        {
            options.AdditionalProperties ??= new AdditionalPropertiesDictionary();
            options.AdditionalProperties["vk.turnId"] = args.TurnId;
        }

        var affinityKey = !string.IsNullOrWhiteSpace(args?.AffinityKey)
            ? args.AffinityKey
            : args?.TurnId;
        if (!string.IsNullOrWhiteSpace(affinityKey))
        {
            options.AdditionalProperties ??= new AdditionalPropertiesDictionary();
            options.AdditionalProperties["AffinityKey"] = affinityKey;
        }

        return options;
    }

    private static string? ResolveDeploymentIfMapped(
        string? modelId,
        VKAIProviderType? provider,
        VKAIMEAIOptions? extensionsOptions)
    {
        if (string.IsNullOrWhiteSpace(modelId) ||
            extensionsOptions is null ||
            extensionsOptions.ProviderSettings.Count == 0)
        {
            return modelId;
        }

        var providerSettings = VKAIMEAIBlock.ResolveProviderOptions(extensionsOptions, provider);
        return providerSettings.ResolveModelOrDeployment(modelId) ?? modelId;
    }
}
