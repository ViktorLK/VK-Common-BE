using System.Text.Json;
using System.Text.Json.Nodes;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

internal sealed class DefaultSchemaProjection : IVKSchemaProjection
{
    public string ProjectToSchema(VKAIEidosResponseContract contract, bool injectNarrativeField = false)
    {
        VKGuard.NotNull(contract);

        if (!injectNarrativeField || string.IsNullOrWhiteSpace(contract.Schema.RawJsonSchema))
        {
            return contract.Schema.RawJsonSchema;
        }

        return InjectNarrativeFieldToSchema(contract.Schema.RawJsonSchema);
    }

    internal static string InjectNarrativeFieldToSchema(string rawJsonSchema)
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

                if (!properties.ContainsKey("narrativeText"))
                {
                    properties["narrativeText"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Natural language response/narrative text for user display. REQUIRED: You MUST insert '§' as a natural speech pause delimiter between spoken phrases or conversational breaks (e.g., '好的§我想想§其实是这样的')."
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
                    if (item?.GetValue<string>() == "narrativeText")
                    {
                        containsNarrative = true;
                        break;
                    }
                }

                if (!containsNarrative)
                {
                    requiredArray.Add("narrativeText");
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
