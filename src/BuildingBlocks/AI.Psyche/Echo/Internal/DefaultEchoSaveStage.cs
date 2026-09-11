using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Echo.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

/// <summary>
/// Pipeline stage executing after LLM response completion to automatically persist dialogue traces into IVKEchoStore.
/// Follows AP.01 (sealed class), CS.01 (Result pattern), CS.03 (ConfigureAwait(false)), and OR.01/BB.04 (Diagnostics & Logging).
/// </summary>
[VKTrace("psyche.stage.echo_save")]
internal sealed class DefaultEchoSaveStage : IVKPsychePipelineStage // [AP.01]
{
    private const string StageName = "EchoSave";

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
        _echoStore = VKGuard.NotNull(echoStore); // [AP.01]
        _modelFactory = VKGuard.NotNull(modelFactory); // [AP.01]
        _tokenCounter = VKGuard.NotNull(tokenCounter); // [AP.01]
        _options = VKGuard.NotNull(options); // [AP.01]
        _logger = VKGuard.NotNull(logger); // [AP.01]
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context); // [AP.01]

        if (context.IsWeaveOnly || context.IsSandbox)
        {
            return VKResult.Success(); // [CS.01]
        }

        var session = context.State<VKSessionThread>();
        if (session is null)
        {
            return VKResult.Success(); // [CS.01]
        }

        var sessionId = session.Id;
        var traces = new List<VKEchoTrace>(2);

        // 1. Auto-save User Input trace (from context.Request.UserInput)
        var userInput = context.Request.UserInput;
        if (!string.IsNullOrWhiteSpace(userInput))
        {
            var userTokens = _tokenCounter.CountTokens(userInput);
            var userTrace = _modelFactory.CreateEcho(sessionId, VKChatRole.User, userInput, tokenCount: userTokens, createdAt: context.CreatedAt);
            traces.Add(userTrace);
            _logger.EchoRecorded(sessionId, VKChatRole.User, userInput.Length); // [OR.01]
        }

        // 2. Auto-save Assistant Response trace (from context.Response.ChatResponse.Message.Content)
        var assistantMsgContent = context.ResponseBuilder.ChatResponse?.Message?.Content;
        if (!string.IsNullOrWhiteSpace(assistantMsgContent))
        {
            var assistantTokens = _tokenCounter.CountTokens(assistantMsgContent);
            var assistantTrace = _modelFactory.CreateEcho(sessionId, VKChatRole.Assistant, assistantMsgContent, tokenCount: assistantTokens);
            traces.Add(assistantTrace);
            _logger.EchoRecorded(sessionId, VKChatRole.Assistant, assistantMsgContent.Length); // [OR.01]
        }

        if (traces.Count > 0)
        {
            var stopwatch = Stopwatch.StartNew();
            var saveResult = await _echoStore.SaveHistoryBatchAsync(traces, cancellationToken).ConfigureAwait(false); // [CS.03]
            stopwatch.Stop();

            var durationMs = stopwatch.Elapsed.TotalMilliseconds;
            var isSuccess = saveResult is null || saveResult.IsSuccess;

            EchoDiagnostics.RecordSave(durationMs, StageName, isSuccess); // [BB.04]

            if (saveResult is { IsFailure: true })
            {
                _logger.EchoSaveFailed(sessionId, saveResult.FirstError.Description); // [OR.01]
                return saveResult; // [CS.01]
            }

            EchoDiagnostics.RecordSavedEchoes(traces.Count, StageName); // [BB.04]
            _logger.EchoSaved(traces.Count, sessionId, durationMs); // [OR.01]
        }

        Activity.Current?.SetPsycheEchoSavedCount(traces.Count); // [BB.04]
        return VKResult.Success(); // [CS.01]
    }
}
