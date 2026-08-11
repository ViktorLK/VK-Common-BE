using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

/// <summary>
/// Pipeline stage executing after LLM response completion to automatically persist dialogue traces into IVKEchoStore.
/// Follows AP.01 (sealed class), CS.03 (ConfigureAwait(false)), and CS.06 (TimeProvider/IVKGuidGenerator).
/// </summary>
internal sealed class DefaultEchoSaveStage : IVKPsychePipelineStage
{
    private readonly IVKEchoStore _echoStore;
    private readonly IVKPsycheModelFactory _modelFactory;
    private readonly VKEchoOptions _options;
    private readonly ILogger<DefaultEchoSaveStage> _logger;

    public bool IsActive => _options.Enabled && _options.AutoSaveHistory;

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.After.PsycheEchoSave;

    public DefaultEchoSaveStage(
        IVKEchoStore echoStore,
        IVKPsycheModelFactory modelFactory,
        VKEchoOptions options,
        ILogger<DefaultEchoSaveStage> logger)
    {
        _echoStore = VKGuard.NotNull(echoStore);
        _modelFactory = VKGuard.NotNull(modelFactory);
        _options = VKGuard.NotNull(options);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            if (context.IsWeaveOnly || context.IsSandbox)
            {
                return VKResult.Success();
            }

            var session = context.State<VKSessionThread>();
            if (session is null)
            {
                return VKResult.Success();
            }

            var sessionId = session.Id;

            // 1. Auto-save User Input trace (from context.Request.UserInput)
            var userInput = context.Request.UserInput;
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                var userTrace = _modelFactory.CreateEcho(sessionId, VKChatRole.User, userInput);
                await _echoStore.SaveTraceAsync(userTrace, cancellationToken).ConfigureAwait(false);
            }

            // 2. Auto-save Assistant Response trace (from context.Response.ChatResponse.Message.Content)
            var assistantMsgContent = context.Response.ChatResponse?.Message?.Content;
            if (!string.IsNullOrWhiteSpace(assistantMsgContent))
            {
                var assistantTrace = _modelFactory.CreateEcho(sessionId, VKChatRole.Assistant, assistantMsgContent);
                await _echoStore.SaveTraceAsync(assistantTrace, cancellationToken).ConfigureAwait(false);
            }

            return VKResult.Success();
        }
        finally
        {
            stopwatch.Stop();
            context.Response.ProfilingMetrics[VKPsycheProfilingKeys.EchoSaveStage] = stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
