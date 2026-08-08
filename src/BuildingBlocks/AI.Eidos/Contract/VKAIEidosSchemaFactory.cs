using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Factory helpers for building VKAIEidosResponseContract and VKAIEidosSchema directly from C# DTO types.
/// </summary>
public static class VKAIEidosSchemaFactory
{
    private static readonly NullabilityInfoContext NullabilityContext = new();

    /// <summary>
    /// Generates a VKAIEidosSchema from a target C# DTO type T using System.Text.Json.Schema,
    /// with optional template variable replacement in descriptions.
    /// </summary>
    public static VKAIEidosSchema FromType<T>(
        string? schemaName = null,
        JsonSerializerOptions? serializerOptions = null,
        IReadOnlyDictionary<string, string>? templateArgs = null)
    {
        return FromType(typeof(T), schemaName, serializerOptions, templateArgs);
    }

    /// <summary>
    /// Generates a VKAIEidosSchema from a target C# DTO type using System.Text.Json.Schema,
    /// with optional template variable replacement in descriptions.
    /// </summary>
    public static VKAIEidosSchema FromType(
        Type targetType,
        string? schemaName = null,
        JsonSerializerOptions? serializerOptions = null,
        IReadOnlyDictionary<string, string>? templateArgs = null)
    {
        VKGuard.NotNull(targetType);

        var name = schemaName ?? targetType.Name;
        var options = serializerOptions ?? JsonSerializerOptions.Default;
        var node = options.GetJsonSchemaAsNode(targetType);

        if (node is JsonObject objNode)
        {
            CleanNullableTypes(objNode);
            if (templateArgs is { Count: > 0 })
            {
                ApplyTemplateReplacements(objNode, templateArgs);
            }
        }

        var requiredList = ExtractRequiredProperties(targetType, node);

        return new VKAIEidosSchema
        {
            SchemaName = name,
            RawJsonSchema = node.ToJsonString(),
            RequiredProperties = requiredList
        };
    }

    /// <summary>
    /// Creates a complete VKAIEidosResponseContract from a target C# DTO type T.
    /// </summary>
    public static VKAIEidosResponseContract CreateContract<T>(
        string scenario,
        string? description = null,
        string? schemaName = null,
        JsonSerializerOptions? serializerOptions = null,
        IReadOnlyDictionary<string, string>? templateArgs = null)
    {
        return CreateContract(typeof(T), scenario, description, schemaName, serializerOptions, templateArgs);
    }

    /// <summary>
    /// Creates a complete VKAIEidosResponseContract from a target C# DTO type.
    /// </summary>
    public static VKAIEidosResponseContract CreateContract(
        Type targetType,
        string scenario,
        string? description = null,
        string? schemaName = null,
        JsonSerializerOptions? serializerOptions = null,
        IReadOnlyDictionary<string, string>? templateArgs = null)
    {
        VKGuard.NotNull(targetType);
        VKGuard.NotNullOrWhiteSpace(scenario);

        var schema = FromType(targetType, schemaName, serializerOptions, templateArgs);

        return new VKAIEidosResponseContract
        {
            ContractId = $"{scenario}:{targetType.Name}",
            Scenario = scenario,
            Description = description ?? $"Response contract generated for {targetType.Name}",
            Version = VKAIEidosContractVersion.V1,
            Schema = schema
        };
    }

    /// <summary>
    /// Merges a base DTO JSON schema string with a dynamic overlay JSON schema string from DB/Registry.
    /// Combines properties and required property arrays.
    /// </summary>
    public static string MergeSchemas(string baseSchemaJson, string overlaySchemaJson)
    {
        if (string.IsNullOrWhiteSpace(baseSchemaJson)) return overlaySchemaJson;
        if (string.IsNullOrWhiteSpace(overlaySchemaJson)) return baseSchemaJson;

        try
        {
            if (JsonNode.Parse(baseSchemaJson) is not JsonObject baseNode ||
                JsonNode.Parse(overlaySchemaJson) is not JsonObject overlayNode)
            {
                return overlaySchemaJson;
            }

            // 1. Merge properties
            if (overlayNode["properties"] is JsonObject overlayProperties)
            {
                if (baseNode["properties"] is not JsonObject baseProperties)
                {
                    baseProperties = [];
                    baseNode["properties"] = baseProperties;
                }

                foreach (var (key, val) in overlayProperties)
                {
                    if (!baseProperties.ContainsKey(key) && val is not null)
                    {
                        baseProperties[key] = val.DeepClone();
                    }
                }
            }

            // 2. Merge required properties
            if (overlayNode["required"] is JsonArray overlayRequired)
            {
                if (baseNode["required"] is not JsonArray baseRequired)
                {
                    baseRequired = [];
                    baseNode["required"] = baseRequired;
                }

                var existingSet = new HashSet<string>(
                    baseRequired.Select(x => x?.GetValue<string>()).OfType<string>(), 
                    StringComparer.OrdinalIgnoreCase);

                foreach (var item in overlayRequired)
                {
                    if (item?.GetValue<string>() is { } reqName && existingSet.Add(reqName))
                    {
                        baseRequired.Add(reqName);
                    }
                }
            }

            return baseNode.ToJsonString();
        }
        catch
        {
            return overlaySchemaJson;
        }
    }

    private static void ApplyTemplateReplacements(JsonNode node, IReadOnlyDictionary<string, string> templateArgs)
    {
        if (node is JsonObject obj)
        {
            if (obj.TryGetPropertyValue("description", out var descNode) && 
                descNode is JsonValue val && 
                val.TryGetValue<string>(out var descStr))
            {
                foreach (var (key, value) in templateArgs)
                {
                    descStr = descStr.Replace($"{{{key}}}", value, StringComparison.OrdinalIgnoreCase);
                }
                obj["description"] = descStr;
            }

            foreach (var (_, child) in obj)
            {
                if (child is not null)
                {
                    ApplyTemplateReplacements(child, templateArgs);
                }
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                if (item is not null)
                {
                    ApplyTemplateReplacements(item, templateArgs);
                }
            }
        }
    }

    private static void CleanNullableTypes(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            if (obj.TryGetPropertyValue("type", out var typeNode) && typeNode is JsonArray typeArr)
            {
                string? nonNullType = null;
                foreach (var item in typeArr)
                {
                    if (item?.GetValue<string>() is { Length: > 0 } typeStr &&
                        !string.Equals(typeStr, "null", StringComparison.OrdinalIgnoreCase))
                    {
                        nonNullType = typeStr;
                        break;
                    }
                }

                if (nonNullType is not null)
                {
                    obj["type"] = nonNullType;
                }
            }

            foreach (var (_, child) in obj)
            {
                if (child is not null)
                {
                    CleanNullableTypes(child);
                }
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                if (item is not null)
                {
                    CleanNullableTypes(item);
                }
            }
        }
    }

    private static List<string> ExtractRequiredProperties(Type targetType, JsonNode? node)
    {
        if (node is not JsonObject obj) return [];

        if (!obj.ContainsKey("required") || obj["required"] is not JsonArray requiredArray)
        {
            requiredArray = [];
            obj["required"] = requiredArray;
        }

        var propertiesMap = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var jsonPropNames = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in propertiesMap)
        {
            var jsonPropAttr = p.GetCustomAttribute<JsonPropertyNameAttribute>();
            var propName = jsonPropAttr?.Name ?? p.Name;
            jsonPropNames[propName] = p;
        }

        if (obj.TryGetPropertyValue("properties", out var propsNode) && propsNode is JsonObject propertiesObj)
        {
            foreach (var (jsonPropName, _) in propertiesObj)
            {
                bool isRequired;
                if (jsonPropNames.TryGetValue(jsonPropName, out var pInfo))
                {
                    var hasRequiredAttr = pInfo.GetCustomAttributes(false)
                        .Any(a => a.GetType().Name is "JsonRequiredAttribute" or "RequiredAttribute");

                    var isNullableType = Nullable.GetUnderlyingType(pInfo.PropertyType) is not null ||
                                         NullabilityContext.Create(pInfo).WriteState == NullabilityState.Nullable;

                    isRequired = hasRequiredAttr || !isNullableType;
                }
                else
                {
                    isRequired = true;
                }

                if (isRequired && !requiredArray.Any(x => x?.GetValue<string>() == jsonPropName))
                {
                    requiredArray.Add(jsonPropName);
                }
            }
        }

        var requiredList = new List<string>(requiredArray.Count);
        foreach (var item in requiredArray)
        {
            if (item?.GetValue<string>() is string reqName)
            {
                requiredList.Add(reqName);
            }
        }

        return requiredList;
    }
}

