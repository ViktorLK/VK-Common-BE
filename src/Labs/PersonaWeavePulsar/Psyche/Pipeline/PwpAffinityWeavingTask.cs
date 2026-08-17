using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Common.DependencyInjection.Internal;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Pipeline;

/// <summary>
/// Weaving task to inject the current affinity score into the prompt tapestry.
/// </summary>
internal sealed class PwpAffinityWeavingTask : IVKWeavingPipelineTask
{
    private readonly PwpContext _pwpContext;

    public PwpAffinityWeavingTask(PwpContext pwpContext)
    {
        _pwpContext = VKGuard.NotNull(pwpContext);
    }

    // Execute after persona definition
    public int TaskOrder => VK.Blocks.AI.Psyche.Weaving.Internal.VKWeavingTaskOrder.Formatter + 50;

    public bool IsParallel => false;
    public int? ParallelGroup => null;

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var score = _pwpContext.CurrentAffinityScore;
        var affinityContent = $"[System Note: The current affinity score with the user is {score}/100. Adjust your responses and tone accordingly.]";

        //context.AddFragment(new VKPromptFragment
        //{
        //    Content = affinityContent,
        //    TierType = VKPromptTierType.Persona,
        //    Metadata = new PwpAffinityMetadata(),
        //    Role = VKChatRole.System,
        //    RenderOrder = 100 // High priority to avoid truncation
        //});

        return Task.FromResult(VKResult.Success());
    }

    private sealed record PwpAffinityMetadata : IVKFragmentMetadata;
}
