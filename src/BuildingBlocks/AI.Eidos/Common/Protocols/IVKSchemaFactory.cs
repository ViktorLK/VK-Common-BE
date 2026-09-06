using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Service contract for generating standardized JSON schemas and interaction response contracts from C# DTO types.
/// </summary>
public interface IVKSchemaFactory
{
    /// <summary>
    /// Generates a <see cref="VKAIEidosSchema"/> from target C# DTO type T.
    /// </summary>
    VKAIEidosSchema FromType<T>(
        string? schemaName = null,
        JsonSerializerOptions? serializerOptions = null,
        IReadOnlyDictionary<string, string>? templateArgs = null);

    /// <summary>
    /// Generates a <see cref="VKAIEidosSchema"/> from a target runtime C# DTO type.
    /// </summary>
    VKAIEidosSchema FromType(
        Type targetType,
        string? schemaName = null,
        JsonSerializerOptions? serializerOptions = null,
        IReadOnlyDictionary<string, string>? templateArgs = null);

    /// <summary>
    /// Creates a complete <see cref="VKAIEidosResponseContract"/> from target C# DTO type T.
    /// </summary>
    VKAIEidosResponseContract CreateContract<T>(
        string contractName,
        string? description = null,
        string? schemaName = null,
        string? version = null,
        JsonSerializerOptions? serializerOptions = null,
        IReadOnlyDictionary<string, string>? templateArgs = null);

    /// <summary>
    /// Creates a complete <see cref="VKAIEidosResponseContract"/> from a target runtime C# DTO type.
    /// </summary>
    VKAIEidosResponseContract CreateContract(
        Type targetType,
        string contractName,
        string? description = null,
        string? schemaName = null,
        string? version = null,
        JsonSerializerOptions? serializerOptions = null,
        IReadOnlyDictionary<string, string>? templateArgs = null);

    /// <summary>
    /// Generates a polymorphic Tagged Union JSON Schema from a base type TBase and its [JsonDerivedType] sub-types.
    /// </summary>
    VKAIEidosSchema FromPolymorphicType<TBase>(
        string? schemaName = null,
        string discriminatorProperty = "type",
        JsonSerializerOptions? serializerOptions = null);

    /// <summary>
    /// Generates a polymorphic Tagged Union JSON Schema from a base type and its [JsonDerivedType] sub-types.
    /// </summary>
    VKAIEidosSchema FromPolymorphicType(
        Type baseType,
        string? schemaName = null,
        string discriminatorProperty = "type",
        JsonSerializerOptions? serializerOptions = null);

    /// <summary>
    /// Merges a base DTO JSON schema string with a dynamic overlay JSON schema string.
    /// </summary>
    string MergeSchemas(string baseSchemaJson, string overlaySchemaJson);

    /// <summary>
    /// Composes a base schema with multiple reusable schema fragments.
    /// </summary>
    VKAIEidosSchema Compose(VKAIEidosSchema baseSchema, params VKAIEidosSchemaFragment[] fragments);

    /// <summary>
    /// Computes a deterministic hexadecimal fingerprint from a JSON schema string.
    /// </summary>
    string ComputeFingerprint(string rawJsonSchema);

    /// <summary>
    /// Computes a deterministic hexadecimal fingerprint from a JsonNode.
    /// </summary>
    string ComputeFingerprint(JsonNode node);
}
