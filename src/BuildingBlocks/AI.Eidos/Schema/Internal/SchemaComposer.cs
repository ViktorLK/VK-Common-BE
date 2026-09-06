using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Schema.Internal;

/// <summary>
/// Internal utility for merging and composing JSON schemas with modular fragments.
/// </summary>
internal static class SchemaComposer
{
    /// <summary>
    /// Merges a base DTO JSON schema string with a dynamic overlay JSON schema string from DB/Registry.
    /// Combines properties and required property arrays with deduplication.
    /// </summary>
    public static string MergeSchemas(string baseSchemaJson, string overlaySchemaJson)
    {
        if (string.IsNullOrWhiteSpace(baseSchemaJson))
            return overlaySchemaJson ?? string.Empty;
        if (string.IsNullOrWhiteSpace(overlaySchemaJson))
            return baseSchemaJson;

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
        catch (Exception)
        {
            // If overlay parsing or merge fails, fall back to base schema or overlay safely
            return !string.IsNullOrWhiteSpace(baseSchemaJson) ? baseSchemaJson : overlaySchemaJson;
        }
    }

    /// <summary>
    /// Composes a base schema with multiple reusable schema fragments (e.g. Pagination, Audit).
    /// Merges properties and required fields, returning a new VKAIEidosSchema with a fresh fingerprint.
    /// </summary>
    public static VKAIEidosSchema Compose(
        VKAIEidosSchema baseSchema,
        params VKAIEidosSchemaFragment[] fragments)
    {
        VKGuard.NotNull(baseSchema);
        if (fragments is null || fragments.Length == 0)
            return baseSchema;

        var mergedJson = baseSchema.RawJsonSchema;
        var requiredList = new HashSet<string>(baseSchema.RequiredProperties, StringComparer.OrdinalIgnoreCase);

        foreach (var frag in fragments)
        {
            if (frag is null)
                continue;
            mergedJson = MergeSchemas(mergedJson, frag.RawJsonSchema);
            foreach (var req in frag.RequiredProperties)
            {
                requiredList.Add(req);
            }
        }

        var fingerprint = SchemaFingerprint.Compute(mergedJson);

        return new VKAIEidosSchema
        {
            SchemaName = baseSchema.SchemaName,
            RawJsonSchema = mergedJson,
            RequiredProperties = [.. requiredList],
            Fingerprint = fingerprint
        };
    }
}
