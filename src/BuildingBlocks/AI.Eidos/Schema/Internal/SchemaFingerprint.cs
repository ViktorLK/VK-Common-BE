using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Schema.Internal;

/// <summary>
/// Internal utility for computing deterministic structural fingerprints of JSON Schemas.
/// </summary>
internal static class SchemaFingerprint
{
    /// <summary>
    /// Computes a deterministic hexadecimal fingerprint from a JSON schema string.
    /// </summary>
    public static string Compute(string rawJsonSchema)
    {
        if (string.IsNullOrWhiteSpace(rawJsonSchema))
            return string.Empty;

        try
        {
            var node = JsonNode.Parse(rawJsonSchema);
            return node is null ? string.Empty : Compute(node);
        }
        catch
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawJsonSchema));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }

    /// <summary>
    /// Computes a deterministic hexadecimal fingerprint from a JsonNode.
    /// </summary>
    public static string Compute(JsonNode node)
    {
        VKGuard.NotNull(node);
        var normalized = NormalizeNode(node);
        var normalizedJson = normalized.ToJsonString();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedJson));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static JsonNode NormalizeNode(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            var sortedObj = new JsonObject();
            var keys = obj.Select(kv => kv.Key).OrderBy(k => k, StringComparer.Ordinal).ToList();
            foreach (var key in keys)
            {
                var child = obj[key];
                if (child is not null)
                {
                    if (string.Equals(key, "required", StringComparison.OrdinalIgnoreCase) && child is JsonArray reqArr)
                    {
                        var sortedReq = new JsonArray();
                        var sortedItems = reqArr
                            .Select(x => x?.GetValue<string>())
                            .OfType<string>()
                            .Distinct(StringComparer.Ordinal)
                            .OrderBy(s => s, StringComparer.Ordinal);
                        foreach (var item in sortedItems)
                        {
                            sortedReq.Add(item);
                        }
                        sortedObj[key] = sortedReq;
                    }
                    else
                    {
                        sortedObj[key] = NormalizeNode(child);
                    }
                }
            }
            return sortedObj;
        }

        if (node is JsonArray arr)
        {
            var newArr = new JsonArray();
            foreach (var item in arr)
            {
                if (item is not null)
                {
                    newArr.Add(NormalizeNode(item));
                }
            }
            return newArr;
        }

        return node.DeepClone();
    }
}
