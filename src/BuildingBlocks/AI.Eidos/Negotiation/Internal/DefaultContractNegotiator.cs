using System;
using System.Text.Json;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

internal sealed class DefaultContractNegotiator(
    IOptions<VKNegotiationOptions>? options = null) : IVKContractNegotiator
{
    private readonly VKNegotiationOptions _options = options?.Value ?? new VKNegotiationOptions();

    public VKAIEidosNegotiationResult Negotiate(
        VKAIEidosResponseContract contract,
        VKAIEidosProviderCapabilities capabilities,
        VKAIEidosExpressionMode? preferredMode = null)
    {
        VKGuard.NotNull(contract);
        VKGuard.NotNull(capabilities);

        var mode = preferredMode ?? _options.DefaultPreferredMode;
        var isComplex = IsComplexSchema(contract.Schema);

        // For complex schemas, prevent degraded PromptJson if native structured output is available
        if (isComplex && _options.DisallowPromptJsonForComplexSchemas && mode == VKAIEidosExpressionMode.PromptJson)
        {
            if (capabilities.SupportsNativeStructuredOutput)
            {
                mode = VKAIEidosExpressionMode.StructuredOutput;
            }
        }

        if (mode == VKAIEidosExpressionMode.StructuredOutput && capabilities.SupportsNativeStructuredOutput)
        {
            return new VKAIEidosNegotiationResult
            {
                SelectedMode = VKAIEidosExpressionMode.StructuredOutput,
                Contract = contract
            };
        }

        if (capabilities.SupportsNativeStructuredOutput && mode != VKAIEidosExpressionMode.PromptJson)
        {
            return new VKAIEidosNegotiationResult
            {
                SelectedMode = VKAIEidosExpressionMode.StructuredOutput,
                Contract = contract
            };
        }

        return new VKAIEidosNegotiationResult
        {
            SelectedMode = VKAIEidosExpressionMode.PromptJson,
            Contract = contract,
            SystemPromptInstruction = $"{NegotiationConstants.StrictJsonDirectivePrefix}{contract.Schema.RawJsonSchema}"
        };
    }

    private bool IsComplexSchema(VKAIEidosSchema schema)
    {
        if (schema.RequiredProperties.Count > _options.ComplexSchemaPropertyThreshold)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(schema.RawJsonSchema))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(schema.RawJsonSchema);
            var root = doc.RootElement;

            // Union discriminator contracts are complex
            if (root.TryGetProperty("oneOf", out _))
            {
                return true;
            }

            // Check for nested objects in properties
            if (root.TryGetProperty("properties", out var props) && props.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in props.EnumerateObject())
                {
                    if (prop.Value.TryGetProperty("type", out var typeElem))
                    {
                        var typeStr = typeElem.GetString();
                        if (typeStr == "object" || typeStr == "array")
                        {
                            return true;
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Fallback on JSON parse error
        }

        return false;
    }
}
