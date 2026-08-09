using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

internal sealed class DefaultPromptProjection : IVKPromptProjection
{
    public string ProjectToPrompt(VKAIEidosResponseContract contract, bool injectNarrativeField = false, bool allowSegmentation = true)
    {
        VKGuard.NotNull(contract);
        var schema = injectNarrativeField
            ? DefaultSchemaProjection.InjectNarrativeFieldToSchema(contract.Schema.RawJsonSchema, allowSegmentation)
            : contract.Schema.RawJsonSchema;

        string narrativeRule = string.Empty;
        if (injectNarrativeField)
        {
            narrativeRule = allowSegmentation
                ? "\n[Narrative Segmentation Protocol]\nIf the response contains natural conversational pauses or distinct spoken phrases, segment 'narrativeSegments' into multiple string elements (e.g., [\"Sure,\", \"Let me check that for you.\", \"Here is the result.\"]). Otherwise, return a single-element array.\n"
                : "\n[Narrative Segmentation Protocol]\nREQUIRED: Return 'narrativeSegments' as a single-element string array containing the complete response text without segmentation (e.g., [\"Sure, here is the complete answer.\"]).\n";
        }

        return $"\n[Response Format Requirement]\nYou MUST respond strictly in JSON format matching this schema:\n{schema}\n{narrativeRule}";
    }
}
