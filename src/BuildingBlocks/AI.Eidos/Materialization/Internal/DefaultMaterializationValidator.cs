using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Materialization.Internal;

internal sealed class DefaultMaterializationValidator : IVKMaterializationValidator // [AP.01]
{
    private const int MaxValidationDepth = 10;

    public VKResult<VKMaterializationValidationResult> Validate(string rawJson, VKAIEidosSchema schema)
    {
        VKGuard.NotNullOrWhiteSpace(rawJson);
        VKGuard.NotNull(schema);

        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return VKResult.Success(VKMaterializationValidationResult.Failure(
                [
                    new VKMaterializationValidationError
                    {
                        Category = VKMaterializationErrorCategory.TypeMismatch,
                        PropertyPath = "$",
                        Expected = "object",
                        Actual = root.ValueKind.ToString(),
                        Message = "Root element is not a JSON object."
                    }
                ]));
            }

            var structuredErrors = new List<VKMaterializationValidationError>();

            // 1. Validate root required properties
            foreach (var prop in schema.RequiredProperties)
            {
                if (!root.TryGetProperty(prop, out _))
                {
                    structuredErrors.Add(new VKMaterializationValidationError
                    {
                        Category = VKMaterializationErrorCategory.MissingRequired,
                        PropertyPath = prop,
                        Message = $"Missing required property '{prop}'."
                    });
                }
            }

            // 2. Validate against schema definition
            if (!string.IsNullOrWhiteSpace(schema.RawJsonSchema))
            {
                try
                {
                    using var schemaDoc = JsonDocument.Parse(schema.RawJsonSchema);
                    var schemaRoot = schemaDoc.RootElement;

                    // Handle Tagged Union / oneOf with Discriminator
                    if (schemaRoot.TryGetProperty("oneOf", out var oneOfElem) && oneOfElem.ValueKind == JsonValueKind.Array)
                    {
                        ValidateUnion(root, schemaRoot, oneOfElem, structuredErrors);
                    }
                    else
                    {
                        ValidateObjectProperties(root, schemaRoot, structuredErrors, string.Empty, 0);
                    }
                }
                catch (JsonException ex)
                {
                    return VKResult.Failure<VKMaterializationValidationResult>(VKAIEidosErrors.Schema.MalformedJsonSchema(ex.Message));
                }
            }

            if (structuredErrors.Count > 0)
            {
                return VKResult.Success(VKMaterializationValidationResult.Failure(structuredErrors));
            }

            return VKResult.Success(VKMaterializationValidationResult.Success());
        }
        catch (JsonException ex)
        {
            return VKResult.Success(VKMaterializationValidationResult.Failure(
            [
                new VKMaterializationValidationError
                {
                    Category = VKMaterializationErrorCategory.Syntax,
                    PropertyPath = "$",
                    Actual = ex.Message,
                    Message = $"Invalid JSON syntax: {ex.Message}"
                }
            ]));
        }
    }

    private static void ValidateUnion(
        JsonElement root,
        JsonElement schemaRoot,
        JsonElement oneOfElem,
        List<VKMaterializationValidationError> structuredErrors)
    {
        string discPropName = "type";
        if (schemaRoot.TryGetProperty("discriminator", out var discElem) &&
            discElem.TryGetProperty("propertyName", out var propNameElem) &&
            propNameElem.ValueKind == JsonValueKind.String)
        {
            discPropName = propNameElem.GetString() ?? "type";
        }

        if (!root.TryGetProperty(discPropName, out var discValElem))
        {
            structuredErrors.Add(new VKMaterializationValidationError
            {
                Category = VKMaterializationErrorCategory.MissingRequired,
                PropertyPath = discPropName,
                Message = $"Missing discriminator property '{discPropName}' required for union contract."
            });
            return;
        }

        var discVal = discValElem.GetString();
        JsonElement? matchingBranch = null;

        foreach (var branch in oneOfElem.EnumerateArray())
        {
            if (branch.TryGetProperty("properties", out var branchProps) &&
                branchProps.TryGetProperty(discPropName, out var branchDiscProp) &&
                branchDiscProp.TryGetProperty("enum", out var enumArr) &&
                enumArr.ValueKind == JsonValueKind.Array)
            {
                if (enumArr.EnumerateArray().Any(e => string.Equals(e.GetString(), discVal, System.StringComparison.OrdinalIgnoreCase)))
                {
                    matchingBranch = branch;
                    break;
                }
            }
        }

        if (matchingBranch is null)
        {
            structuredErrors.Add(new VKMaterializationValidationError
            {
                Category = VKMaterializationErrorCategory.ConstraintViolation,
                PropertyPath = discPropName,
                Actual = discVal,
                Message = $"Unknown discriminator value '{discVal}' for union contract."
            });
            return;
        }

        ValidateObjectProperties(root, matchingBranch.Value, structuredErrors, string.Empty, 0);
    }

    private static void ValidateObjectProperties(
        JsonElement root,
        JsonElement targetSchemaNode,
        List<VKMaterializationValidationError> structuredErrors,
        string currentPath = "",
        int depth = 0)
    {
        if (depth > MaxValidationDepth) return;

        bool strictNoAdditional = targetSchemaNode.TryGetProperty("additionalProperties", out var addPropElem) &&
                                  addPropElem.ValueKind == JsonValueKind.False;

        if (targetSchemaNode.TryGetProperty("required", out var branchRequired) && branchRequired.ValueKind == JsonValueKind.Array)
        {
            foreach (var reqItem in branchRequired.EnumerateArray())
            {
                var reqPropName = reqItem.GetString();
                if (reqPropName is not null && !root.TryGetProperty(reqPropName, out _))
                {
                    var fullReqPath = string.IsNullOrEmpty(currentPath) ? reqPropName : $"{currentPath}.{reqPropName}";
                    if (!structuredErrors.Any(e => e.PropertyPath == fullReqPath && e.Category == VKMaterializationErrorCategory.MissingRequired))
                    {
                        structuredErrors.Add(new VKMaterializationValidationError
                        {
                            Category = VKMaterializationErrorCategory.MissingRequired,
                            PropertyPath = fullReqPath,
                            Message = $"Missing required property '{fullReqPath}'."
                        });
                    }
                }
            }
        }

        if (targetSchemaNode.TryGetProperty("properties", out var propertiesElement) &&
            propertiesElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in root.EnumerateObject())
            {
                var fullPropPath = string.IsNullOrEmpty(currentPath) ? prop.Name : $"{currentPath}.{prop.Name}";

                if (propertiesElement.TryGetProperty(prop.Name, out var schemaPropDef))
                {
                    bool isPropRequired = targetSchemaNode.TryGetProperty("required", out var reqArray) &&
                                          reqArray.ValueKind == JsonValueKind.Array &&
                                          reqArray.EnumerateArray().Any(r => r.GetString() == prop.Name);

                    // Null value check for nullable/optional properties
                    if (prop.Value.ValueKind == JsonValueKind.Null)
                    {
                        bool isNullable = !isPropRequired ||
                                          (schemaPropDef.TryGetProperty("nullable", out var nullElem) && nullElem.ValueKind == JsonValueKind.True) ||
                                          (schemaPropDef.TryGetProperty("type", out var typeArrElem) &&
                                           typeArrElem.ValueKind == JsonValueKind.Array &&
                                           typeArrElem.EnumerateArray().Any(t => t.GetString() == "null"));

                        if (!isNullable)
                        {
                            structuredErrors.Add(new VKMaterializationValidationError
                            {
                                Category = VKMaterializationErrorCategory.TypeMismatch,
                                PropertyPath = fullPropPath,
                                Expected = "non-null",
                                Actual = "Null",
                                Message = $"Property '{fullPropPath}' cannot be null."
                            });
                        }
                        continue;
                    }

                    // Type matching check (handles string or array type definition)
                    if (schemaPropDef.TryGetProperty("type", out var expectedTypeElem))
                    {
                        if (expectedTypeElem.ValueKind == JsonValueKind.String)
                        {
                            var expectedType = expectedTypeElem.GetString();
                            if (!IsTypeMatch(prop.Value.ValueKind, expectedType))
                            {
                                structuredErrors.Add(new VKMaterializationValidationError
                                {
                                    Category = VKMaterializationErrorCategory.TypeMismatch,
                                    PropertyPath = fullPropPath,
                                    Expected = expectedType,
                                    Actual = prop.Value.ValueKind.ToString(),
                                    Message = $"Property '{fullPropPath}' kind '{prop.Value.ValueKind}' does not match expected JSON type '{expectedType}'."
                                });
                            }
                        }
                        else if (expectedTypeElem.ValueKind == JsonValueKind.Array)
                        {
                            var allowedTypes = expectedTypeElem.EnumerateArray().Select(t => t.GetString()).OfType<string>().ToList();
                            if (!allowedTypes.Any(t => IsTypeMatch(prop.Value.ValueKind, t)))
                            {
                                structuredErrors.Add(new VKMaterializationValidationError
                                {
                                    Category = VKMaterializationErrorCategory.TypeMismatch,
                                    PropertyPath = fullPropPath,
                                    Expected = string.Join(" | ", allowedTypes),
                                    Actual = prop.Value.ValueKind.ToString(),
                                    Message = $"Property '{fullPropPath}' kind '{prop.Value.ValueKind}' does not match any allowed JSON type in [{string.Join(", ", allowedTypes)}]."
                                });
                            }
                        }
                    }

                    // Enum constraints check
                    if (schemaPropDef.TryGetProperty("enum", out var enumElement) &&
                        enumElement.ValueKind == JsonValueKind.Array)
                    {
                        var actualValue = prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : prop.Value.GetRawText();
                        var allowedEnums = enumElement.EnumerateArray()
                            .Select(e => e.GetString())
                            .OfType<string>()
                            .ToList();

                        if (actualValue is null || !allowedEnums.Contains(actualValue))
                        {
                            structuredErrors.Add(new VKMaterializationValidationError
                            {
                                Category = VKMaterializationErrorCategory.ConstraintViolation,
                                PropertyPath = fullPropPath,
                                Expected = string.Join(", ", allowedEnums),
                                Actual = actualValue ?? "null",
                                Message = $"Property '{fullPropPath}' value '{actualValue ?? "null"}' is not in allowed enum values: [{string.Join(", ", allowedEnums)}]."
                            });
                        }
                    }

                    // Recursive nested object validation
                    if (prop.Value.ValueKind == JsonValueKind.Object && schemaPropDef.ValueKind == JsonValueKind.Object)
                    {
                        ValidateObjectProperties(prop.Value, schemaPropDef, structuredErrors, fullPropPath, depth + 1);
                    }
                    // Recursive nested array validation
                    else if (prop.Value.ValueKind == JsonValueKind.Array &&
                             schemaPropDef.TryGetProperty("items", out var itemsSchema) &&
                             itemsSchema.ValueKind == JsonValueKind.Object)
                    {
                        var itemIdx = 0;
                        foreach (var itemElement in prop.Value.EnumerateArray())
                        {
                            var itemPath = $"{fullPropPath}[{itemIdx}]";
                            ValidateElementAgainstSchema(itemElement, itemsSchema, structuredErrors, itemPath, depth + 1);
                            itemIdx++;
                        }
                    }
                }
                else if (strictNoAdditional)
                {
                    structuredErrors.Add(new VKMaterializationValidationError
                    {
                        Category = VKMaterializationErrorCategory.ExtraneousFields,
                        PropertyPath = fullPropPath,
                        Message = $"Extraneous property '{fullPropPath}' is not allowed by contract schema."
                    });
                }
            }
        }
    }

    private static void ValidateElementAgainstSchema(
        JsonElement element,
        JsonElement schemaNode,
        List<VKMaterializationValidationError> structuredErrors,
        string currentPath,
        int depth)
    {
        if (depth > MaxValidationDepth) return;

        if (element.ValueKind == JsonValueKind.Null)
        {
            bool isNullable = (schemaNode.TryGetProperty("nullable", out var nullElem) && nullElem.ValueKind == JsonValueKind.True) ||
                              (schemaNode.TryGetProperty("type", out var typeArrElem) &&
                               typeArrElem.ValueKind == JsonValueKind.Array &&
                               typeArrElem.EnumerateArray().Any(t => t.GetString() == "null"));
            if (!isNullable)
            {
                structuredErrors.Add(new VKMaterializationValidationError
                {
                    Category = VKMaterializationErrorCategory.TypeMismatch,
                    PropertyPath = currentPath,
                    Expected = "non-null",
                    Actual = "Null",
                    Message = $"Item at '{currentPath}' cannot be null."
                });
            }
            return;
        }

        if (schemaNode.TryGetProperty("type", out var expectedTypeElem))
        {
            if (expectedTypeElem.ValueKind == JsonValueKind.String)
            {
                var expectedType = expectedTypeElem.GetString();
                if (!IsTypeMatch(element.ValueKind, expectedType))
                {
                    structuredErrors.Add(new VKMaterializationValidationError
                    {
                        Category = VKMaterializationErrorCategory.TypeMismatch,
                        PropertyPath = currentPath,
                        Expected = expectedType,
                        Actual = element.ValueKind.ToString(),
                        Message = $"Item at '{currentPath}' kind '{element.ValueKind}' does not match expected JSON type '{expectedType}'."
                    });
                }
            }
            else if (expectedTypeElem.ValueKind == JsonValueKind.Array)
            {
                var allowedTypes = expectedTypeElem.EnumerateArray().Select(t => t.GetString()).OfType<string>().ToList();
                if (!allowedTypes.Any(t => IsTypeMatch(element.ValueKind, t)))
                {
                    structuredErrors.Add(new VKMaterializationValidationError
                    {
                        Category = VKMaterializationErrorCategory.TypeMismatch,
                        PropertyPath = currentPath,
                        Expected = string.Join(" | ", allowedTypes),
                        Actual = element.ValueKind.ToString(),
                        Message = $"Item at '{currentPath}' kind '{element.ValueKind}' does not match any allowed JSON type in [{string.Join(", ", allowedTypes)}]."
                    });
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            ValidateObjectProperties(element, schemaNode, structuredErrors, currentPath, depth + 1);
        }
        else if (element.ValueKind == JsonValueKind.Array &&
                 schemaNode.TryGetProperty("items", out var nestedItemsSchema) &&
                 nestedItemsSchema.ValueKind == JsonValueKind.Object)
        {
            var itemIdx = 0;
            foreach (var childElement in element.EnumerateArray())
            {
                var itemPath = $"{currentPath}[{itemIdx}]";
                ValidateElementAgainstSchema(childElement, nestedItemsSchema, structuredErrors, itemPath, depth + 1);
                itemIdx++;
            }
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
