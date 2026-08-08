using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

internal sealed class DefaultPromptProjection : IVKPromptProjection
{
    public string ProjectToPrompt(VKAIEidosResponseContract contract, bool injectNarrativeField = false)
    {
        VKGuard.NotNull(contract);
        var schema = injectNarrativeField
            ? DefaultSchemaProjection.InjectNarrativeFieldToSchema(contract.Schema.RawJsonSchema)
            : contract.Schema.RawJsonSchema;

        var narrativeRule = injectNarrativeField
            ? "\n[Narrative Speech Rule]\nWhen generating 'narrativeText', insert '§' as a speech pause delimiter between spoken phrases to simulate natural human pauses (Example: \"好的§让我思考一下§其实逻辑很清晰。\").\n"
            : string.Empty;

        return $"\n[Response Format Requirement]\nYou MUST respond strictly in JSON format matching this schema:\n{schema}\n{narrativeRule}";
    }
}
