using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent.EgressText.Internal;

internal sealed class DefaultEgressPacer : IVKEgressPacer
{
    private readonly VKEgressTextOptions _textOptions;
    private static readonly Random _random = new();

    public DefaultEgressPacer(IOptionsSnapshot<VKEgressTextOptions> textOptions)
    {
        _textOptions = VKGuard.NotNull(textOptions?.Value);
    }

    public VKResult<IReadOnlyList<VKEgressPacingChunk>> CalculatePacing(IReadOnlyList<string> segments, VKEgressTextOptions? overrideOptions = null)
    {
        VKGuard.NotNull(segments);

        var options = overrideOptions ?? _textOptions;
        if (segments.Count == 0)
        {
            IReadOnlyList<VKEgressPacingChunk> emptyResult = [];
            return VKResult.Success(emptyResult);
        }

        // Single element array -> Plain text mode (delay_ms = 0)
        if (segments.Count == 1)
        {
            IReadOnlyList<VKEgressPacingChunk> singleChunk = [
                new VKEgressPacingChunk
                {
                    Text = segments[0],
                    DelayMs = 0,
                    SequenceIndex = 0,
                    IsFinal = true
                }
            ];
            return VKResult.Success(singleChunk);
        }

        List<VKEgressPacingChunk> chunks = new(segments.Count);
        for (int i = 0; i < segments.Count; i++)
        {
            var segmentText = segments[i];
            int delayMs = CalculateSegmentDelay(segmentText, options, isFirst: i == 0);

            chunks.Add(new VKEgressPacingChunk
            {
                Text = segmentText,
                DelayMs = delayMs,
                SequenceIndex = i,
                IsFinal = i == segments.Count - 1
            });
        }

        IReadOnlyList<VKEgressPacingChunk> resultList = chunks.AsReadOnly();
        return VKResult.Success(resultList);
    }

    private static int CalculateSegmentDelay(string segment, VKEgressTextOptions options, bool isFirst)
    {
        int charCount = segment.Length;
        int delay = charCount * options.BaseCharDelayMs;

        if (isFirst)
        {
            delay += options.InitialThinkingDelayMs;
        }

        char lastChar = segment[^1];
        if (lastChar == '\n')
        {
            delay += options.ParagraphEndDelayMs;
        }
        else if ("。？！.?!".Contains(lastChar))
        {
            delay += options.SentenceEndDelayMs;
        }

        if (options.JitterFactor > 0.0)
        {
            double factor = 1.0 + ((_random.NextDouble() * 2.0 - 1.0) * Math.Clamp(options.JitterFactor, 0.0, 0.5));
            delay = (int)(delay * factor);
        }

        return Math.Max(10, delay);
    }
}
