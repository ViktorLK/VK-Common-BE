using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

internal sealed class DefaultContractProjector(
    IVKToolProjection toolProjection,
    IVKSchemaProjection schemaProjection,
    IVKPromptProjection promptProjection) : IVKContractProjector
{
    private readonly IVKToolProjection _toolProjection = VKGuard.NotNull(toolProjection);
    private readonly IVKSchemaProjection _schemaProjection = VKGuard.NotNull(schemaProjection);
    private readonly IVKPromptProjection _promptProjection = VKGuard.NotNull(promptProjection);

    public object ProjectToIntermediateRepresentation(
        VKAIEidosResponseContract contract,
        VKAIEidosExpressionMode mode,
        bool injectNarrativeField = false)
    {
        VKGuard.NotNull(contract);

        return mode switch
        {
            VKAIEidosExpressionMode.ToolCall => _toolProjection.ProjectToTool(contract, injectNarrativeField),
            VKAIEidosExpressionMode.PromptJson => _promptProjection.ProjectToPrompt(contract, injectNarrativeField),
            _ => _schemaProjection.ProjectToSchema(contract, injectNarrativeField)
        };
    }
}
