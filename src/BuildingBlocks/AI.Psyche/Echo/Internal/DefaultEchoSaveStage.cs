using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

/// <summary>
/// Pipeline stage executing after LLM response completion to automatically persist dialogue traces into IVKEchoStore.
/// Follows AP.01 (sealed class), CS.03 (ConfigureAwait(false)), and CS.06 (TimeProvider/IVKGuidGenerator).
/// </summary>
[VKTrace("psyche.stage.echo_save")]
internal sealed class DefaultEchoSaveStage : IVKPsychePipelineStage
{
    private readonly IVKEchoStore _echoStore;
    private readonly IVKPsycheModelFactory _modelFactory;
    private readonly IVKTokenCounter _tokenCounter;
    private readonly VKEchoOptions _options;
    private readonly ILogger<DefaultEchoSaveStage> _logger;

    public bool IsActive => _options.Enabled && _options.AutoSaveHistory;

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.After.PsycheEchoSave;

    public DefaultEchoSaveStage(
        IVKEchoStore echoStore,
        IVKPsycheModelFactory modelFactory,
        IVKTokenCounter tokenCounter,
        VKEchoOptions options,
        ILogger<DefaultEchoSaveStage> logger)
    {
        _echoStore = VKGuard.NotNull(echoStore);
        _modelFactory = VKGuard.NotNull(modelFactory);
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _options = VKGuard.NotNull(options);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

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
        var traces = new List<VKEchoTrace>(2);

        // 1. Auto-save User Input trace (from context.Request.UserInput)
        var userInput = context.Request.UserInput;
        if (!string.IsNullOrWhiteSpace(userInput))
        {
            var userTokens = _tokenCounter.CountTokens(userInput);
            traces.Add(_modelFactory.CreateEcho(sessionId, VKChatRole.User, userInput, tokenCount: userTokens, createdAt: context.CreatedAt));
        }

        // 2. Auto-save Assistant Response trace (from context.Response.ChatResponse.Message.Content)
        var assistantMsgContent = context.ResponseBuilder.ChatResponse?.Message?.Content;
        if (!string.IsNullOrWhiteSpace(assistantMsgContent))
        {
            var assistantTokens = context.ResponseBuilder.ChatResponse?.Usage?.OutputTokens > 0
                ? (int)context.ResponseBuilder.ChatResponse.Usage.OutputTokens
                : _tokenCounter.CountTokens(assistantMsgContent);
            traces.Add(_modelFactory.CreateEcho(sessionId, VKChatRole.Assistant, assistantMsgContent, tokenCount: assistantTokens));
        }

        if (traces.Count > 0)
        {
            await _echoStore.SaveHistoryBatchAsync(traces, cancellationToken).ConfigureAwait(false);
        }

        return VKResult.Success();
    }
}
