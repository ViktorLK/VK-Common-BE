using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

/// <summary>
/// Pipeline stage in Psyche Before phase detecting capabilities, negotiating format, and projecting schemas.
/// </summary>
internal sealed class DefaultNegotiationStage(
    IVKProviderCapabilityDetector capabilityDetector,
    IVKContractNegotiator negotiator,
    IVKContractProjector projector) : IVKPsychePipelineStage
{
    private readonly IVKProviderCapabilityDetector _capabilityDetector = VKGuard.NotNull(capabilityDetector);
    private readonly IVKContractNegotiator _negotiator = VKGuard.NotNull(negotiator);
    private readonly IVKContractProjector _projector = VKGuard.NotNull(projector);

    public VKPipelineSchedule Schedule => new(400, false, null, VKPipelinePhase.Before);
    public bool IsActive => true;

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        var contract = context.State<VKAIEidosResponseContract>();
        if (contract is null)
        {
            return Task.FromResult(VKResult.Success());
        }

        var eidosArgs = context.Args<VKAIEidosRequestArgs>();
        var chatArgs = context.Args<VKChatArgs>() ?? new VKChatArgs();
        var provider = chatArgs.Provider ?? VKAIProviderType.OpenAI;
        var modelId = chatArgs.ModelId ?? VKAIModelIds.OpenAI.Gpt4O;

        var capabilities = _capabilityDetector.DetectCapabilities(provider, modelId);
        var negotiationResult = _negotiator.Negotiate(contract, capabilities, eidosArgs?.PreferredMode);
        context.SetState(negotiationResult);

        var projectedChatArgs = _projector.ApplyProjection(
            chatArgs,
            contract,
            negotiationResult.SelectedMode);
        context.SetArgs(projectedChatArgs);

        if (negotiationResult.SelectedMode == VKAIEidosExpressionMode.PromptJson)
        {
            context.AddFragment(_projector.GetHeaderProtocolFragment());
            context.AddFragment(_projector.GetTailSchemaFragment(contract));
        }

        return Task.FromResult(VKResult.Success());
    }
}
