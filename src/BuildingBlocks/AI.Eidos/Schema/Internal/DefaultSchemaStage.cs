using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Schema.Internal;

/// <summary>
/// Pipeline stage in Psyche Before phase resolving and preparing response contracts.
/// </summary>
internal sealed class DefaultSchemaStage(IVKSchemaResolver resolver) : IVKPsychePipelineStage
{
    private readonly IVKSchemaResolver _resolver = VKGuard.NotNull(resolver);

    public VKPipelineSchedule Schedule => new(300, false, null, VKPipelinePhase.Before);
    public bool IsActive => true;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        var contract = context.State<VKAIEidosResponseContract>()
            ?? await _resolver.ResolveFromArgsAsync(context.Args<VKAIEidosRequestArgs>(), cancellationToken: cancellationToken).ConfigureAwait(false);

        if (contract is not null)
        {
            context.SetState(contract);
        }

        return VKResult.Success();
    }
}
