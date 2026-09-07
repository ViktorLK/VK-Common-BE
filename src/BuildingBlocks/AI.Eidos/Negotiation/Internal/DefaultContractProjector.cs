using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

internal sealed class DefaultContractProjector(
    IVKGuidGenerator guidGenerator,
    IOptions<VKNegotiationOptions>? options = null) : IVKContractProjector
{
    private readonly IVKGuidGenerator _guidGenerator = VKGuard.NotNull(guidGenerator);
    private readonly VKNegotiationOptions _options = options?.Value ?? new VKNegotiationOptions();

    private static readonly VKPatternId HeaderPatternId = VKPatternId.Parse("e1d05000-0000-0000-0000-000000000001");

    private static readonly Lazy<VKPromptFragment> StaticHeaderFragment = new(() =>
    {
        var headerSegment = new VKPromptSegment
        {
            Role = VKChatRole.System,
            Content = NegotiationConstants.StrictJsonProtocolContent,
            RelativeDepth = VKPromptRelativeDepth.AfterDirective,
            DepthPriority = 950
        };
        var headerPattern = VKPatternEntry.Create(HeaderPatternId, headerSegment).Value!;

        return new VKPromptFragment
        {
            TierType = VKPromptTierType.Pattern,
            Metadata = headerPattern,
            Segment = headerSegment
        };
    }, LazyThreadSafetyMode.PublicationOnly);

    private readonly ConcurrentDictionary<string, Lazy<VKPromptFragment>> _tailPatternCache = new();
    private readonly ConcurrentDictionary<string, string> _promptInstructionCache = new();
    private readonly ConcurrentDictionary<string, string> _projectedSchemaCache = new();

    public string ProjectToIntermediateRepresentation(
        VKAIEidosResponseContract contract,
        VKAIEidosExpressionMode mode)
    {
        VKGuard.NotNull(contract);

        return mode switch
        {
            VKAIEidosExpressionMode.PromptJson => GetOrBuildPromptInstruction(contract),
            _ => contract.Schema.RawJsonSchema
        };
    }

    public VKChatArgs ApplyProjection(
        VKChatArgs chatArgs,
        VKAIEidosResponseContract contract,
        VKAIEidosExpressionMode mode)
    {
        VKGuard.NotNull(chatArgs);
        VKGuard.NotNull(contract);

        var provider = chatArgs.Provider ?? VKAIModelIds.ResolveProvider(chatArgs.ModelId);

        return mode switch
        {
            VKAIEidosExpressionMode.StructuredOutput => chatArgs with
            {
                ResponseSchema = GetOrBuildProjectedSchema(contract, provider)
            },
            _ => chatArgs
        };
    }

    public VKPromptFragment GetHeaderProtocolFragment() => StaticHeaderFragment.Value;

    public VKPromptFragment GetTailSchemaFragment(VKAIEidosResponseContract contract)
    {
        VKGuard.NotNull(contract);
        var cacheKey = $"{contract.ContractId}:{contract.Version}";

        return _tailPatternCache.GetOrAdd(cacheKey, _ => new Lazy<VKPromptFragment>(() =>
        {
            var schemaDirective = GetOrBuildPromptInstruction(contract);
            var schemaSegment = new VKPromptSegment
            {
                Role = VKChatRole.System,
                Content = schemaDirective,
                AbsoluteDepth = 0,
                DepthPriority = 950
            };
            var schemaPattern = VKPatternEntry.Create(VKPatternId.New(_guidGenerator), schemaSegment).Value!;

            return new VKPromptFragment
            {
                TierType = VKPromptTierType.Pattern,
                Metadata = schemaPattern,
                Segment = schemaSegment
            };
        }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    private string GetOrBuildPromptInstruction(VKAIEidosResponseContract contract)
    {
        var cacheKey = $"{contract.ContractId}:{contract.Version}";
        return _promptInstructionCache.GetOrAdd(cacheKey, _ => BuildPromptInstruction(contract.Schema.RawJsonSchema));
    }

    private string BuildPromptInstruction(string rawJsonSchema)
    {
        var fewShotExample = string.Empty;
        if (_options.InjectPromptFewShotExample)
        {
            var exampleJson = SynthesizeExampleJson(rawJsonSchema);
            if (!string.IsNullOrWhiteSpace(exampleJson))
            {
                fewShotExample = $"\n{NegotiationConstants.ExampleFormatHeader}\n```json\n{exampleJson}\n```\n";
            }
        }

        return $"\n{NegotiationConstants.ResponseFormatHeader}\nYou MUST respond strictly in JSON format matching this schema:\n{rawJsonSchema}{fewShotExample}";
    }

    private static string? SynthesizeExampleJson(string rawJsonSchema)
    {
        if (string.IsNullOrWhiteSpace(rawJsonSchema))
        {
            return null;
        }

        try
        {
            var node = JsonNode.Parse(rawJsonSchema);
            if (node is JsonObject rootObj)
            {
                var exampleObj = GenerateExampleFromSchemaNode(rootObj);
                return exampleObj.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            }
        }
        catch (JsonException)
        {
            // Fallback on JSON parse error
        }

        return null;
    }

    private static JsonNode GenerateExampleFromSchemaNode(JsonObject schemaNode)
    {
        if (schemaNode.TryGetPropertyValue("oneOf", out var oneOfNode) && oneOfNode is JsonArray oneOfArr && oneOfArr.Count > 0)
        {
            if (oneOfArr[0] is JsonObject firstBranch)
            {
                return GenerateExampleFromSchemaNode(firstBranch);
            }
        }

        var result = new JsonObject();
        if (schemaNode.TryGetPropertyValue("properties", out var propsNode) && propsNode is JsonObject propsObj)
        {
            foreach (var (propName, propDefNode) in propsObj)
            {
                if (propDefNode is JsonObject propDef)
                {
                    result[propName] = SynthesizeValue(propDef);
                }
            }
        }

        return result;
    }

    private static JsonNode SynthesizeValue(JsonObject propDef)
    {
        if (propDef.TryGetPropertyValue("enum", out var enumNode) && enumNode is JsonArray enumArr && enumArr.Count > 0)
        {
            return enumArr[0]?.DeepClone() ?? JsonValue.Create("value");
        }

        var typeStr = propDef.TryGetPropertyValue("type", out var typeNode) && typeNode is JsonValue val && val.TryGetValue<string>(out var t) ? t : "string";

        return typeStr switch
        {
            "string" => JsonValue.Create("sample_value"),
            "number" => JsonValue.Create(0.0),
            "integer" => JsonValue.Create(1),
            "boolean" => JsonValue.Create(true),
            "array" => SynthesizeArray(propDef),
            "object" => GenerateExampleFromSchemaNode(propDef),
            _ => JsonValue.Create("sample_value")
        };
    }

    private static JsonArray SynthesizeArray(JsonObject propDef)
    {
        var arr = new JsonArray();
        if (propDef.TryGetPropertyValue("items", out var itemsNode) && itemsNode is JsonObject itemsObj)
        {
            arr.Add(SynthesizeValue(itemsObj));
        }
        return arr;
    }

    private string GetOrBuildProjectedSchema(VKAIEidosResponseContract contract, VKAIProviderType? provider)
    {
        var cacheKey = $"{contract.ContractId}:{contract.Version}:{provider?.ToString() ?? "Standard"}";
        return _projectedSchemaCache.GetOrAdd(cacheKey, _ => ProjectSchemaForProvider(contract.Schema.RawJsonSchema, provider));
    }

    private static string ProjectSchemaForProvider(string rawJsonSchema, VKAIProviderType? provider)
    {
        if (string.IsNullOrWhiteSpace(rawJsonSchema))
        {
            return rawJsonSchema;
        }

        try
        {
            var node = JsonNode.Parse(rawJsonSchema);
            if (node is not JsonObject root)
            {
                return rawJsonSchema;
            }

            switch (provider)
            {
                case VKAIProviderType.OpenAI:
                case VKAIProviderType.AzureOpenAI:
                    SanitizeJsonSchemaForOpenAi(root);
                    return root.ToJsonString();

                default:
                    CleanSchemaHeaders(root);
                    return root.ToJsonString();
            }
        }
        catch (JsonException)
        {
            return rawJsonSchema;
        }
    }

    private static void CleanSchemaHeaders(JsonObject obj)
    {
        obj.Remove("$schema");
        obj.Remove("$id");
    }

    private static void SanitizeJsonSchemaForOpenAi(JsonObject obj)
    {
        // 1. Remove metadata forbidden by OpenAI Structured Outputs
        obj.Remove("$schema");
        obj.Remove("$id");
        obj.Remove("default");

        // 2. Objects must set additionalProperties: false and list all properties in required
        if (obj.TryGetPropertyValue("properties", out var propsNode) && propsNode is JsonObject props)
        {
            obj["additionalProperties"] = false;

            var existingRequired = new HashSet<string>(StringComparer.Ordinal);
            if (obj.TryGetPropertyValue("required", out var reqNode) && reqNode is JsonArray reqArr)
            {
                foreach (var item in reqArr)
                {
                    if (item?.GetValue<string>() is string reqName)
                    {
                        existingRequired.Add(reqName);
                    }
                }
            }

            var newRequired = new JsonArray();
            foreach (var (propName, propVal) in props)
            {
                newRequired.Add(propName);

                if (propVal is JsonObject propObj)
                {
                    SanitizeJsonSchemaForOpenAi(propObj);

                    // OpenAI strict mode requires all properties to be in 'required'.
                    // Properties not originally required must have their type include 'null'.
                    if (!existingRequired.Contains(propName))
                    {
                        EnsureNullableType(propObj);
                    }
                }
            }

            obj["required"] = newRequired;
        }

        // 3. Array items
        if (obj.TryGetPropertyValue("items", out var itemsNode) && itemsNode is JsonObject itemsObj)
        {
            SanitizeJsonSchemaForOpenAi(itemsObj);
        }

        // 4. Definitions / defs
        if (obj.TryGetPropertyValue("$defs", out var defsNode) && defsNode is JsonObject defs)
        {
            foreach (var (_, defVal) in defs)
            {
                if (defVal is JsonObject defObj)
                {
                    SanitizeJsonSchemaForOpenAi(defObj);
                }
            }
        }

        if (obj.TryGetPropertyValue("definitions", out var legacyDefsNode) && legacyDefsNode is JsonObject legacyDefs)
        {
            foreach (var (_, defVal) in legacyDefs)
            {
                if (defVal is JsonObject defObj)
                {
                    SanitizeJsonSchemaForOpenAi(defObj);
                }
            }
        }
    }

    private static void EnsureNullableType(JsonObject propObj)
    {
        if (propObj.TryGetPropertyValue("type", out var typeNode))
        {
            if (typeNode is JsonValue val && val.TryGetValue<string>(out var typeStr))
            {
                if (!string.Equals(typeStr, "null", StringComparison.OrdinalIgnoreCase))
                {
                    propObj["type"] = new JsonArray { typeStr, "null" };
                }
            }
            else if (typeNode is JsonArray arr)
            {
                var hasNull = arr.Any(x => string.Equals(x?.GetValue<string>(), "null", StringComparison.OrdinalIgnoreCase));
                if (!hasNull)
                {
                    arr.Add("null");
                }
            }
        }
        else if (propObj.TryGetPropertyValue("anyOf", out var anyOfNode) && anyOfNode is JsonArray anyOfArr)
        {
            var hasNull = anyOfArr.Any(x => x is JsonObject o && o.TryGetPropertyValue("type", out var t) && string.Equals(t?.GetValue<string>(), "null", StringComparison.OrdinalIgnoreCase));
            if (!hasNull)
            {
                anyOfArr.Add(new JsonObject { ["type"] = "null" });
            }
        }
    }
}
