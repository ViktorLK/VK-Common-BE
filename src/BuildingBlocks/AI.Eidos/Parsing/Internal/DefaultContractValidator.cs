using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Parsing.Internal;

internal sealed class DefaultContractValidator : IVKContractValidator
{
    public VKResult<VKAIEidosValidationResult> Validate(string rawJson, VKAIEidosSchema schema)
    {
        VKGuard.NotNullOrWhiteSpace(rawJson);
        VKGuard.NotNull(schema);

        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return VKResult.Success(new VKAIEidosValidationResult
                {
                    IsValid = false,
                    ErrorMessages = ["Root element is not a JSON object."]
                });
            }

            var missing = new List<string>();
            var errors = new List<string>();

            foreach (var prop in schema.RequiredProperties)
            {
                if (!root.TryGetProperty(prop, out _))
                {
                    missing.Add(prop);
                    errors.Add($"Missing required property '{prop}'.");
                }
            }

            // Attempt basic schema type validation if RawJsonSchema contains property definitions
            if (!string.IsNullOrWhiteSpace(schema.RawJsonSchema))
            {
                try
                {
                    using var schemaDoc = JsonDocument.Parse(schema.RawJsonSchema);
                    if (schemaDoc.RootElement.TryGetProperty("properties", out var propertiesElement) &&
                        propertiesElement.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in root.EnumerateObject())
                        {
                            if (propertiesElement.TryGetProperty(prop.Name, out var schemaPropDef) &&
                                schemaPropDef.TryGetProperty("type", out var expectedTypeElem) &&
                                expectedTypeElem.ValueKind == JsonValueKind.String)
                            {
                                var expectedType = expectedTypeElem.GetString();
                                if (!IsTypeMatch(prop.Value.ValueKind, expectedType))
                                {
                                    errors.Add($"Property '{prop.Name}' kind '{prop.Value.ValueKind}' does not match expected JSON type '{expectedType}'.");
                                }
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // Ignore schema parse failure in light validator
                }
            }

            return VKResult.Success(new VKAIEidosValidationResult
            {
                IsValid = errors.Count == 0,
                MissingProperties = missing,
                ErrorMessages = errors
            });
        }
        catch (JsonException ex)
        {
            return VKResult.Success(new VKAIEidosValidationResult
            {
                IsValid = false,
                ErrorMessages = [$"Invalid JSON syntax: {ex.Message}"]
            });
        }
    }

    private static bool IsTypeMatch(JsonValueKind kind, string? expectedType)
    {
        return expectedType switch
        {
            "string" => kind == JsonValueKind.String,
            "number" or "integer" => kind == JsonValueKind.Number,
            "boolean" => kind == JsonValueKind.True || kind == JsonValueKind.False,
            "array" => kind == JsonValueKind.Array,
            "object" => kind == JsonValueKind.Object,
            "null" => kind == JsonValueKind.Null,
            _ => true
        };
    }
}
