using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Globalization;
using System.Net.Http.Headers;
using Mscc.GenerativeAI.Types;

namespace VK.Blocks.AI.MEAI.Common.Shared;

/// <summary>
/// Extracts positive <c>Retry-After</c> / <c>retry-after-ms</c> delays from HTTP responses and SDK exceptions.
/// </summary>
internal static class AIMEAIRetryAfterHelper
{
    public static TimeSpan? Extract(HttpResponseHeaders? headers, TimeProvider timeProvider)
    {
        if (headers is null)
        {
            return null;
        }

        if (headers.RetryAfter is { } retryAfter)
        {
            TimeSpan? fromHeader = ExtractCondition(retryAfter, timeProvider);
            if (fromHeader.HasValue)
            {
                return fromHeader;
            }
        }

        if (headers.TryGetValues("retry-after-ms", out var msValues))
        {
            foreach (string rawMs in msValues)
            {
                TimeSpan? parsedMs = ParseMilliseconds(rawMs);
                if (parsedMs.HasValue)
                {
                    return parsedMs;
                }
            }
        }

        return null;
    }

    public static TimeSpan? Extract(PipelineResponseHeaders? headers, TimeProvider timeProvider)
    {
        if (headers is null)
        {
            return null;
        }

        if (headers.TryGetValue("Retry-After", out string? rawRetryAfter))
        {
            TimeSpan? parsed = ParseRawValue(rawRetryAfter, timeProvider);
            if (parsed.HasValue)
            {
                return parsed;
            }
        }

        if (headers.TryGetValue("retry-after-ms", out string? rawMs))
        {
            TimeSpan? parsedMs = ParseMilliseconds(rawMs);
            if (parsedMs.HasValue)
            {
                return parsedMs;
            }
        }

        return null;
    }

    public static TimeSpan? Extract(Exception? exception, TimeProvider timeProvider)
    {
        if (exception is null)
        {
            return null;
        }

        if (exception.Data.Contains(VKAIErrors.RetryAfterMsMetadataKey))
        {
            TimeSpan? fromData = ParseMetadataMilliseconds(exception.Data[VKAIErrors.RetryAfterMsMetadataKey]);
            if (fromData.HasValue)
            {
                return fromData;
            }
        }

        return exception switch
        {
            ClientResultException clientEx => Extract(clientEx.GetRawResponse()?.Headers, timeProvider),
            GeminiApiTimeoutException geminiTimeoutEx => Extract(geminiTimeoutEx.Response?.Headers, timeProvider),
            GeminiApiException geminiEx => Extract(geminiEx.Response?.Headers, timeProvider),
            _ => null
        };
    }

    private static TimeSpan? ExtractCondition(RetryConditionHeaderValue condition, TimeProvider timeProvider)
    {
        if (condition.Delta is { } delta && delta > TimeSpan.Zero)
        {
            return delta;
        }

        if (condition.Date is { } date)
        {
            TimeSpan diff = date - timeProvider.GetUtcNow();
            return diff > TimeSpan.Zero ? diff : null;
        }

        return null;
    }

    private static TimeSpan? ParseRawValue(string? rawValue, TimeProvider timeProvider)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        if (RetryConditionHeaderValue.TryParse(rawValue, out RetryConditionHeaderValue? parsed))
        {
            return ExtractCondition(parsed, timeProvider);
        }

        if (double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds) &&
            double.IsFinite(seconds) &&
            seconds > 0)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        return null;
    }

    private static TimeSpan? ParseMilliseconds(string? rawValue)
    {
        if (!string.IsNullOrWhiteSpace(rawValue) &&
            double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double ms) &&
            double.IsFinite(ms) &&
            ms > 0)
        {
            return TimeSpan.FromMilliseconds(ms);
        }

        return null;
    }

    private static TimeSpan? ParseMetadataMilliseconds(object? rawValue)
    {
        double? ms = rawValue switch
        {
            double d => d,
            int i => i,
            long l => l,
            float f => f,
            _ => null
        };

        if (ms is { } value && double.IsFinite(value) && value > 0)
        {
            return value >= TimeSpan.MaxValue.TotalMilliseconds
                ? TimeSpan.MaxValue
                : TimeSpan.FromMilliseconds(value);
        }

        return null;
    }
}
