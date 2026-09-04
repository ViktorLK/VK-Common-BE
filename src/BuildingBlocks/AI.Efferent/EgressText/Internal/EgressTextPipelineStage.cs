using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent.EgressText.Internal;

internal sealed class EgressTextPipelineStage : IVKPsychePipelineStage
{
    private readonly IVKEgressTextFormatter _outputFormatter;
    private readonly IVKEgressPacer _pacer;
    private readonly VKEgressTextOptions _options;
    private readonly ILogger<EgressTextPipelineStage> _logger;

    public bool IsActive => _options.Enabled;

    public VKPipelineSchedule Schedule => new(200, false, null, VKPipelinePhase.After); // Executes after EgressGuardrails (100)

    public EgressTextPipelineStage(
        IVKEgressTextFormatter outputFormatter,
        IVKEgressPacer pacer,
        IOptionsSnapshot<VKEgressTextOptions> options,
        ILogger<EgressTextPipelineStage> logger)
    {
        _outputFormatter = VKGuard.NotNull(outputFormatter);
        _pacer = VKGuard.NotNull(pacer);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var segments = ExtractSegments(context);
        if (segments is { Count: > 0 })
        {
            var combinedText = string.Join(string.Empty, segments);
            var updatedMsg = new VKChatMessage { Role = VKChatRole.Assistant, Content = combinedText };
            context.ResponseBuilder.ChatResponse = (context.ResponseBuilder.ChatResponse ?? new VKChatResponse { Message = updatedMsg }) with { Message = updatedMsg };

            if (_options.EnablePacing)
            {
                var pacingResult = _pacer.CalculatePacing(segments, _options);
                if (pacingResult.IsSuccess)
                {
                    context.ResponseBuilder.Metadata["VKEgressPacingChunks"] = pacingResult.Value;
                }
            }

            return VKResult.Success();
        }

        var rawContent = ExtractRawContent(context);
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return VKResult.Success();
        }

        var formatResult = await _outputFormatter.FormatOutputAsync(rawContent, cancellationToken).ConfigureAwait(false);
        if (formatResult.IsFailure)
        {
            return VKResult.Failure(formatResult.FirstError);
        }

        var formattedText = formatResult.Value;
        if (formattedText != rawContent && context.ResponseBuilder.ChatResponse?.Message is not null)
        {
            var originalMsg = context.ResponseBuilder.ChatResponse.Message;
            var updatedMsg = originalMsg with { Content = formattedText };
            context.ResponseBuilder.ChatResponse = context.ResponseBuilder.ChatResponse with { Message = updatedMsg };
        }

        if (_options.EnablePacing)
        {
            var pacingResult = _pacer.CalculatePacing([formattedText], _options);
            if (pacingResult.IsSuccess)
            {
                context.ResponseBuilder.Metadata["VKEgressPacingChunks"] = pacingResult.Value;
            }
        }

        return VKResult.Success();
    }

    private static IReadOnlyList<string>? ExtractSegments(VKPsycheContext context)
    {
        if (context.ResponseBuilder.ModelResult is not null)
        {
            var prop = context.ResponseBuilder.ModelResult.GetType().GetProperty("DialogueSegments", BindingFlags.Public | BindingFlags.Instance)
                       ?? context.ResponseBuilder.ModelResult.GetType().GetProperty("NarrativeSegments", BindingFlags.Public | BindingFlags.Instance);
            if (prop?.GetValue(context.ResponseBuilder.ModelResult) is IReadOnlyList<string> list && list.Count > 0)
            {
                return list;
            }
        }

        return null;
    }

    private static string? ExtractRawContent(VKPsycheContext context)
    {
        if (context.ResponseBuilder.ModelResult is not null)
        {
            var prop = context.ResponseBuilder.ModelResult.GetType().GetProperty("NarrativeText", BindingFlags.Public | BindingFlags.Instance);
            if (prop?.GetValue(context.ResponseBuilder.ModelResult) is string textFromProp && !string.IsNullOrWhiteSpace(textFromProp))
            {
                return textFromProp;
            }
        }

        return context.ResponseBuilder.ChatResponse?.Message.Content;
    }
}
