using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

/// <summary>
/// Pipeline stage executing after LLM response completion to automatically persist dialogue traces into IVKEchoStore.
/// Follows AP.01 (sealed class), CS.03 (ConfigureAwait(false)), and CS.06 (TimeProvider/IVKGuidGenerator).
/// </summary>
internal sealed class EchoSavePipelineStage : IVKPsychePipelineStage
{
    private readonly IVKEchoStore _echoStore;
    private readonly IVKIdentityContext _identityContext;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly VKEchoOptions _options;
    private readonly ILogger<EchoSavePipelineStage> _logger;

    public bool IsActive => _options.Enabled && _options.AutoSaveHistory;

    public VKPipelineSchedule Schedule => new(900, false, null, VKPipelinePhase.After);

    public EchoSavePipelineStage(
        IVKEchoStore echoStore,
        IVKIdentityContext identityContext,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider,
        IOptionsSnapshot<VKEchoOptions> options,
        ILogger<EchoSavePipelineStage> logger)
    {
        _echoStore = VKGuard.NotNull(echoStore);
        _identityContext = VKGuard.NotNull(identityContext);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // CS Sandbox isolation rule: Skip permanent DB side-effects in sandbox mode
        if (context.IsSandbox)
        {
            return VKResult.Success();
        }

        var sessionId = context.Request.SessionId;
        if (sessionId.Value == Guid.Empty)
        {
            return VKResult.Success();
        }

        var now = _timeProvider.GetUtcNow();

        // 1. Auto-save User Input trace (from context.Request.UserInput)
        var userInput = context.Request.UserInput;
        if (!string.IsNullOrWhiteSpace(userInput))
        {
            var userTrace = new VKEchoTrace
            {
                TenantId = _identityContext.TenantId,
                SessionId = sessionId,
                Id = new VKEchoId(_guidGenerator.Create()),
                Role = VKChatRole.User,
                Content = userInput,
                Timestamp = now
            };

            await _echoStore.SaveTraceAsync(userTrace, cancellationToken).ConfigureAwait(false);
        }

        // 2. Auto-save Assistant Response trace (from context.Response.ChatResponse.Message.Content)
        var assistantMsgContent = context.Response.ChatResponse?.Message?.Content;
        if (!string.IsNullOrWhiteSpace(assistantMsgContent))
        {
            var assistantTrace = new VKEchoTrace
            {
                TenantId = _identityContext.TenantId,
                SessionId = sessionId,
                Id = new VKEchoId(_guidGenerator.Create()),
                Role = VKChatRole.Assistant,
                Content = assistantMsgContent,
                Timestamp = now
            };

            await _echoStore.SaveTraceAsync(assistantTrace, cancellationToken).ConfigureAwait(false);
        }

        return VKResult.Success();
    }
}
