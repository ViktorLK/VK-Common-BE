using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using VK.Blocks.AI.Eidos.Schema.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Fluent DSL builder for dynamically constructing JSON schemas and contracts without static C# types.
/// </summary>
public sealed class VKDynamicSchemaBuilder
{
    private readonly JsonObject _properties = [];
    private readonly List<string> _requiredProperties = [];

    /// <summary>
    /// Adds a string property to the dynamic schema.
    /// </summary>
    public VKDynamicSchemaBuilder AddString(
        string name,
        string? description = null,
        bool required = false,
        IReadOnlyList<string>? enumValues = null)
    {
        VKGuard.NotNullOrWhiteSpace(name);

        var propObj = new JsonObject
        {
            ["type"] = "string"
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            propObj["description"] = description;
        }

        if (enumValues is { Count: > 0 })
        {
            var enumArray = new JsonArray();
            foreach (var val in enumValues)
            {
                enumArray.Add(val);
            }
            propObj["enum"] = enumArray;
        }

        _properties[name] = propObj;

        if (required && !_requiredProperties.Contains(name))
        {
            _requiredProperties.Add(name);
        }

        return this;
    }

    /// <summary>
    /// Adds a numeric property (number or integer) to the dynamic schema.
    /// </summary>
    public VKDynamicSchemaBuilder AddNumber(
        string name,
        string? description = null,
        bool required = false,
        bool isInteger = false)
    {
        VKGuard.NotNullOrWhiteSpace(name);

        var propObj = new JsonObject
        {
            ["type"] = isInteger ? "integer" : "number"
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            propObj["description"] = description;
        }

        _properties[name] = propObj;

        if (required && !_requiredProperties.Contains(name))
        {
            _requiredProperties.Add(name);
        }

        return this;
    }

    /// <summary>
    /// Adds a boolean property to the dynamic schema.
    /// </summary>
    public VKDynamicSchemaBuilder AddBoolean(
        string name,
        string? description = null,
        bool required = false)
    {
        VKGuard.NotNullOrWhiteSpace(name);

        var propObj = new JsonObject
        {
            ["type"] = "boolean"
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            propObj["description"] = description;
        }

        _properties[name] = propObj;

        if (required && !_requiredProperties.Contains(name))
        {
            _requiredProperties.Add(name);
        }

        return this;
    }

    /// <summary>
    /// Adds an array property to the dynamic schema.
    /// </summary>
    public VKDynamicSchemaBuilder AddArray(
        string name,
        string itemType = "string",
        string? description = null,
        bool required = false)
    {
        VKGuard.NotNullOrWhiteSpace(name);

        var propObj = new JsonObject
        {
            ["type"] = "array",
            ["items"] = new JsonObject
            {
                ["type"] = itemType
            }
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            propObj["description"] = description;
        }

        _properties[name] = propObj;

        if (required && !_requiredProperties.Contains(name))
        {
            _requiredProperties.Add(name);
        }

        return this;
    }

    /// <summary>
    /// Adds a nested object property configured via a nested builder.
    /// </summary>
    public VKDynamicSchemaBuilder AddObject(
        string name,
        Action<VKDynamicSchemaBuilder> configureNested,
        string? description = null,
        bool required = false)
    {
        VKGuard.NotNullOrWhiteSpace(name);
        VKGuard.NotNull(configureNested);

        var nestedBuilder = new VKDynamicSchemaBuilder();
        configureNested(nestedBuilder);
        var nestedSchemaNode = nestedBuilder.BuildNode();

        if (!string.IsNullOrWhiteSpace(description))
        {
            nestedSchemaNode["description"] = description;
        }

        _properties[name] = nestedSchemaNode;

        if (required && !_requiredProperties.Contains(name))
        {
            _requiredProperties.Add(name);
        }

        return this;
    }

    /// <summary>
    /// Builds the internal JSON Schema JsonObject.
    /// </summary>
    public JsonObject BuildNode()
    {
        var rootNode = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = _properties.DeepClone()
        };

        if (_requiredProperties.Count > 0)
        {
            var reqArr = new JsonArray();
            foreach (var req in _requiredProperties.OrderBy(x => x, StringComparer.Ordinal))
            {
                reqArr.Add(req);
            }
            rootNode["required"] = reqArr;
        }

        return rootNode;
    }

    /// <summary>
    /// Builds a strongly-typed <see cref="VKAIEidosSchema"/> with an automatically computed fingerprint.
    /// </summary>
    public VKAIEidosSchema BuildSchema(string schemaName = "DynamicSchema")
    {
        VKGuard.NotNullOrWhiteSpace(schemaName);

        var node = BuildNode();
        var rawJson = node.ToJsonString();
        var fingerprint = SchemaFingerprint.Compute(node);

        return new VKAIEidosSchema
        {
            SchemaName = schemaName,
            RawJsonSchema = rawJson,
            RequiredProperties = _requiredProperties.ToList(),
            Fingerprint = fingerprint
        };
    }

    /// <summary>
    /// Builds a complete <see cref="VKAIEidosResponseContract"/> for scenario routing.
    /// </summary>
    public VKAIEidosResponseContract BuildContract(
        string contractName,
        string? description = null,
        string schemaName = "DynamicSchema",
        string version = "1.0")
    {
        VKGuard.NotNullOrWhiteSpace(contractName);

        var schema = BuildSchema(schemaName);
        var resolvedVersion = string.IsNullOrWhiteSpace(version) ? "1.0" : version;

        return new VKAIEidosResponseContract
        {
            ContractId = $"{contractName}:{schema.SchemaName}:{resolvedVersion}",
            ContractName = contractName,
            Description = description ?? $"Dynamically built contract for {contractName}",
            Version = resolvedVersion,
            Schema = schema
        };
    }
}
