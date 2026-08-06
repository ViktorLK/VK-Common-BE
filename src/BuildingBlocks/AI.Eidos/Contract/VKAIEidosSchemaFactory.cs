using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Factory helpers for building VKAIEidosResponseContract and VKAIEidosSchema directly from C# DTO types.
/// </summary>
public static class VKAIEidosSchemaFactory
{
    /// <summary>
    /// Generates a VKAIEidosSchema from a target C# DTO type T using System.Text.Json.Schema.
    /// </summary>
    public static VKAIEidosSchema FromType<T>(
        string? schemaName = null,
        JsonSerializerOptions? serializerOptions = null)
    {
        return FromType(typeof(T), schemaName, serializerOptions);
    }

    /// <summary>
    /// Generates a VKAIEidosSchema from a target C# DTO type using System.Text.Json.Schema.
    /// </summary>
    public static VKAIEidosSchema FromType(
        Type targetType,
        string? schemaName = null,
        JsonSerializerOptions? serializerOptions = null)
    {
        VKGuard.NotNull(targetType);

        var name = schemaName ?? targetType.Name;
        var node = serializerOptions is not null
            ? serializerOptions.GetJsonSchemaAsNode(targetType)
            : JsonSerializerOptions.Default.GetJsonSchemaAsNode(targetType);

        var requiredList = new List<string>();

        if (node is JsonObject obj)
        {
            if (obj.TryGetPropertyValue("required", out var requiredNode) && requiredNode is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    if (item?.GetValue<string>() is string reqName)
                    {
                        requiredList.Add(reqName);
                    }
                }
            }
        }

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
        JsonSerializerOptions? serializerOptions = null)
    {
        return CreateContract(typeof(T), scenario, description, schemaName, serializerOptions);
    }

    /// <summary>
    /// Creates a complete VKAIEidosResponseContract from a target C# DTO type.
    /// </summary>
    public static VKAIEidosResponseContract CreateContract(
        Type targetType,
        string scenario,
        string? description = null,
        string? schemaName = null,
        JsonSerializerOptions? serializerOptions = null)
    {
        VKGuard.NotNull(targetType);
        VKGuard.NotNullOrWhiteSpace(scenario);

        var schema = FromType(targetType, schemaName, serializerOptions);

        return new VKAIEidosResponseContract
        {
            ContractId = $"{scenario}:{targetType.Name}",
            Scenario = scenario,
            Description = description ?? $"Response contract generated for {targetType.Name}",
            Version = VKAIEidosContractVersion.V1,
            Schema = schema
        };
    }
}
