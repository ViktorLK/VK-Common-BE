using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Eidos.Schema.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Common.Internal;

/// <summary>
/// Default implementation of <see cref="IVKSchemaFactory"/> with high-performance reflection caching and JSON Schema header injection.
/// </summary>
internal sealed class DefaultSchemaFactory : IVKSchemaFactory
{
    private static readonly NullabilityInfoContext NullabilityContext = new();
    private static readonly JsonSchemaExporterOptions DefaultExporterOptions = new()
    {
        TransformSchemaNode = (context, schema) =>
        {
            if (schema is not JsonObject obj)
            {
                return schema;
            }

            if (context.PropertyInfo is { } propInfo)
            {
                var descAttr = propInfo.AttributeProvider?
                    .GetCustomAttributes(typeof(DescriptionAttribute), inherit: true)
                    .OfType<DescriptionAttribute>()
                    .FirstOrDefault();

                if (descAttr is not null && !string.IsNullOrWhiteSpace(descAttr.Description) && !obj.ContainsKey("description"))
                {
                    obj["description"] = descAttr.Description;
                }
            }
            else if (context.TypeInfo is { } typeInfo && !obj.ContainsKey("description"))
            {
                var typeDescAttr = typeInfo.Type.GetCustomAttribute<DescriptionAttribute>(inherit: true);
                if (typeDescAttr is not null && !string.IsNullOrWhiteSpace(typeDescAttr.Description))
                {
                    obj["description"] = typeDescAttr.Description;
                }
            }

            return schema;
        }
    };
    private readonly ConcurrentDictionary<string, VKAIEidosSchema> _schemaCache = new(StringComparer.Ordinal);
    private readonly VKSchemaOptions _options;

    public DefaultSchemaFactory(IOptions<VKSchemaOptions>? options = null)
    {
        _options = options?.Value ?? new VKSchemaOptions();
    }

    public VKAIEidosSchema FromType<T>(
        string? schemaName = null,
        JsonSerializerOptions? serializerOptions = null,
        IReadOnlyDictionary<string, string>? templateArgs = null)
    {
        return FromType(typeof(T), schemaName, serializerOptions, templateArgs);
    }

    public VKAIEidosSchema FromType(
        Type targetType,
        string? schemaName = null,
        JsonSerializerOptions? serializerOptions = null,
        IReadOnlyDictionary<string, string>? templateArgs = null)
    {
        VKGuard.NotNull(targetType);

        var name = schemaName ?? targetType.Name;
        var cacheKey = BuildCacheKey(targetType, name, serializerOptions, templateArgs);

        if (_options.EnableSchemaCache && _schemaCache.TryGetValue(cacheKey, out var cachedSchema))
        {
            return cachedSchema;
        }

        var options = serializerOptions ?? JsonSerializerOptions.Default;
        var node = options.GetJsonSchemaAsNode(targetType, DefaultExporterOptions);

        if (node is JsonObject objNode)
        {
            CleanNullableTypes(objNode);

            if (_options.InjectSchemaHeader)
            {
                InjectSchemaHeaders(objNode, name);
            }
        }

        var requiredList = ExtractRequiredProperties(targetType, node);

        if (node is JsonObject rootObj && templateArgs is { Count: > 0 })
        {
            ApplyTemplateReplacements(rootObj, templateArgs);
        }
        var rawJson = node.ToJsonString();
        var fingerprint = ComputeFingerprint(node);

        var result = new VKAIEidosSchema
        {
            SchemaName = name,
            RawJsonSchema = rawJson,
            RequiredProperties = requiredList,
            Fingerprint = fingerprint
        };

        if (_options.EnableSchemaCache)
        {
            _schemaCache[cacheKey] = result;
        }

        return result;
    }

    public VKAIEidosResponseContract CreateContract<T>(
        string contractName,
        string? description = null,
        string? schemaName = null,
        string? version = null,
        JsonSerializerOptions? serializerOptions = null,
        IReadOnlyDictionary<string, string>? templateArgs = null)
    {
        return CreateContract(typeof(T), contractName, description, schemaName, version, serializerOptions, templateArgs);
    }

    public VKAIEidosResponseContract CreateContract(
        Type targetType,
        string contractName,
        string? description = null,
        string? schemaName = null,
        string? version = null,
        JsonSerializerOptions? serializerOptions = null,
        IReadOnlyDictionary<string, string>? templateArgs = null)
    {
        VKGuard.NotNull(targetType);
        VKGuard.NotNullOrWhiteSpace(contractName);

        var schema = FromType(targetType, schemaName, serializerOptions, templateArgs);
        var resolvedVersion = string.IsNullOrWhiteSpace(version) ? "1.0" : version;

        return new VKAIEidosResponseContract
        {
            ContractId = $"{contractName}:{targetType.Name}:{resolvedVersion}",
            ContractName = contractName,
            Description = description ?? $"Response contract generated for {targetType.Name}",
            Version = resolvedVersion,
            Schema = schema
        };
    }

    public VKAIEidosSchema FromPolymorphicType<TBase>(
        string? schemaName = null,
        string discriminatorProperty = "type",
        JsonSerializerOptions? serializerOptions = null)
    {
        return FromPolymorphicType(typeof(TBase), schemaName, discriminatorProperty, serializerOptions);
    }

    public VKAIEidosSchema FromPolymorphicType(
        Type baseType,
        string? schemaName = null,
        string discriminatorProperty = "type",
        JsonSerializerOptions? serializerOptions = null)
    {
        VKGuard.NotNull(baseType);
        VKGuard.NotNullOrWhiteSpace(discriminatorProperty);

        var name = schemaName ?? baseType.Name;
        var derivedAttributes = baseType.GetCustomAttributes<JsonDerivedTypeAttribute>().ToList();

        var oneOfArray = new JsonArray();

        if (derivedAttributes.Count > 0)
        {
            foreach (var attr in derivedAttributes)
            {
                var derivedType = attr.DerivedType;
                var typeDiscriminatorValue = attr.TypeDiscriminator?.ToString() ?? derivedType.Name;

                var branchSchema = FromType(derivedType, derivedType.Name, serializerOptions);
                var branchDoc = JsonNode.Parse(branchSchema.RawJsonSchema);

                if (branchDoc is JsonObject branchObj)
                {
                    if (branchObj["properties"] is not JsonObject props)
                    {
                        props = [];
                        branchObj["properties"] = props;
                    }

                    // Inject fixed discriminator enum
                    props[discriminatorProperty] = new JsonObject
                    {
                        ["type"] = "string",
                        ["enum"] = new JsonArray { typeDiscriminatorValue }
                    };

                    if (branchObj["required"] is not JsonArray reqArr)
                    {
                        reqArr = [];
                        branchObj["required"] = reqArr;
                    }

                    if (!reqArr.Any(x => x?.GetValue<string>() == discriminatorProperty))
                    {
                        reqArr.Add(discriminatorProperty);
                    }

                    oneOfArray.Add(branchObj);
                }
            }
        }
        else
        {
            return FromType(baseType, name, serializerOptions);
        }

        var rootNode = new JsonObject
        {
            ["type"] = "object",
            ["discriminator"] = new JsonObject
            {
                ["propertyName"] = discriminatorProperty
            },
            ["oneOf"] = oneOfArray
        };

        if (_options.InjectSchemaHeader)
        {
            InjectSchemaHeaders(rootNode, name);
        }

        var rawJson = rootNode.ToJsonString();
        var fingerprint = ComputeFingerprint(rootNode);

        return new VKAIEidosSchema
        {
            SchemaName = name,
            RawJsonSchema = rawJson,
            RequiredProperties = [discriminatorProperty],
            Fingerprint = fingerprint
        };
    }

    public string MergeSchemas(string baseSchemaJson, string overlaySchemaJson)
    {
        return SchemaComposer.MergeSchemas(baseSchemaJson, overlaySchemaJson);
    }

    public VKAIEidosSchema Compose(
        VKAIEidosSchema baseSchema,
        params VKAIEidosSchemaFragment[] fragments)
    {
        return SchemaComposer.Compose(baseSchema, fragments);
    }

    public string ComputeFingerprint(string rawJsonSchema)
    {
        return SchemaFingerprint.Compute(rawJsonSchema);
    }

    public string ComputeFingerprint(JsonNode node)
    {
        return SchemaFingerprint.Compute(node);
    }

    private void InjectSchemaHeaders(JsonObject objNode, string schemaName)
    {
        if (!objNode.ContainsKey("$schema"))
        {
            objNode["$schema"] = "https://json-schema.org/draft/2020-12/schema";
        }

        if (!objNode.ContainsKey("$id"))
        {
            var baseUri = _options.SchemaIdBaseUri?.TrimEnd('/') ?? "https://vkblocks.io/schemas";
            objNode["$id"] = $"{baseUri}/{schemaName}.json";
        }
    }

    private static string BuildCacheKey(
        Type targetType,
        string schemaName,
        JsonSerializerOptions? serializerOptions,
        IReadOnlyDictionary<string, string>? templateArgs)
    {
        var typeKey = targetType.AssemblyQualifiedName ?? targetType.FullName ?? targetType.Name;
        var optHash = serializerOptions?.GetHashCode() ?? 0;
        var argsHash = 0;

        if (templateArgs is { Count: > 0 })
        {
            foreach (var (k, v) in templateArgs)
            {
                argsHash = HashCode.Combine(argsHash, k, v);
            }
        }

        return $"{typeKey}:{schemaName}:{optHash}:{argsHash}";
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
        if (node is not JsonObject obj)
            return [];

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
            foreach (var (jsonPropName, propNode) in propertiesObj)
            {
                bool isRequired;
                if (jsonPropNames.TryGetValue(jsonPropName, out var pInfo))
                {
                    if (propNode is JsonObject propObj && !propObj.ContainsKey("description"))
                    {
                        var descAttr = pInfo.GetCustomAttribute<DescriptionAttribute>();
                        if (descAttr is not null && !string.IsNullOrWhiteSpace(descAttr.Description))
                        {
                            propObj["description"] = descAttr.Description;
                        }
                    }

                    var hasRequiredAttr = pInfo.GetCustomAttribute<JsonRequiredAttribute>() is not null ||
                                         pInfo.GetCustomAttribute<RequiredAttribute>() is not null;

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
