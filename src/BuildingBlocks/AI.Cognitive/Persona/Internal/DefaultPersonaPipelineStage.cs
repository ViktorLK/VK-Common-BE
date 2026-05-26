using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Persona.Internal;

internal sealed class DefaultPersonaPipelineStage : IVKOrchestrationPipelineStage
{
    private readonly VKPersonaOptions _options;

    public int Order => 300;

    public bool IsActive => _options.Enabled;

    public bool IsParallel => true;

    public int? ParallelGroup => 1;

    public DefaultPersonaPipelineStage(IOptions<VKPersonaOptions> options)
    {
        _options = VKGuard.NotNull(options).Value;
    }

    public Task ExecuteAsync(VKOrchestrationPipelineContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        if (string.IsNullOrWhiteSpace(context.PersonaId))
        {
            context.CriticalError = VKPipelineError.From<DefaultPersonaPipelineStage>("PersonaId is required");
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
