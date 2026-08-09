using System.Text.Json;
using System.Text.Json.Nodes;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

internal sealed class DefaultSchemaProjection : IVKSchemaProjection
{
    public string ProjectToSchema(VKAIEidosResponseContract contract, bool injectNarrativeField = false, bool allowSegmentation = true)
    {
        VKGuard.NotNull(contract);

        if (!injectNarrativeField || string.IsNullOrWhiteSpace(contract.Schema.RawJsonSchema))
        {
            return contract.Schema.RawJsonSchema;
        }

        return InjectNarrativeFieldToSchema(contract.Schema.RawJsonSchema, allowSegmentation);
    }

    internal static string InjectNarrativeFieldToSchema(string rawJsonSchema, bool allowSegmentation = true)
    {
        try
        {
            var node = JsonNode.Parse(rawJsonSchema);
            if (node is JsonObject obj)
            {
                if (!obj.ContainsKey("properties") || obj["properties"] is not JsonObject properties)
                {
                    properties = new JsonObject();
                    obj["properties"] = properties;
                }

                var descriptionText = allowSegmentation
                    ? "Natural language response segments for user display. Split into multiple array elements if natural conversational pauses or spoken phrase breaks exist; otherwise, return a single-element array."
                    : "Natural language response text for user display. MUST return a single-element array containing the entire un-segmented response.";

                if (!properties.ContainsKey("narrativeSegments"))
                {
                    properties["narrativeSegments"] = new JsonObject
                    {
                        ["type"] = "array",
                        ["items"] = new JsonObject { ["type"] = "string" },
                        ["description"] = descriptionText
                    };
                }

                if (!obj.ContainsKey("required") || obj["required"] is not JsonArray requiredArray)
                {
                    requiredArray = new JsonArray();
                    obj["required"] = requiredArray;
                }

                bool containsNarrative = false;
                foreach (var item in requiredArray)
                {
                    if (item?.GetValue<string>() == "narrativeSegments")
                    {
                        containsNarrative = true;
                        break;
                    }
                }

                if (!containsNarrative)
                {
                    requiredArray.Add("narrativeSegments");
                }

                return obj.ToJsonString();
            }
        }
        catch
        {
            // Fallback to raw schema on JSON parse error
        }

        return rawJsonSchema;
    }
}
