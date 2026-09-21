using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Observability;

/// <summary>
/// Guards against high-cardinality metric dimensions.
/// Enforces: unbounded values (UserId, email, request ID, etc.) MUST NOT be metric tags.
/// Complies with Manifest §8 (High-Cardinality Dimension Prohibition).
/// </summary>
// [AP.01] sealed/static
// [AP.03] Level 1 Public API with VK prefix
public static class VKMetricDimensionGuard
{
    private static readonly FrozenSet<string> ProhibitedDimensions = new[]
    {
        FieldNames.UserId,
        FieldNames.UserName,
        FieldNames.CorrelationId,
        FieldNames.TraceId,
        FieldNames.SpanId,
        FieldNames.ParentSpanId,
        "user.id",
        "user_id",
        "userid",
        "user.name",
        "user_name",
        "username",
        "email",
        "user.email",
        "user_email",
        "request.id",
        "request_id",
        "session.id",
        "session_id",
        "prompt",
        "query"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the given key is a prohibited high-cardinality dimension.
    /// </summary>
    public static bool IsProhibited(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return ProhibitedDimensions.Contains(key);
    }

    /// <summary>
    /// Validates that the given tag key does not violate high-cardinality constraints.
    /// Throws <see cref="ArgumentException"/> if the tag is prohibited.
    /// </summary>
    // [AP.01] Boundary validation
    public static void ValidateTag(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key, nameof(key));

        if (IsProhibited(key))
        {
            throw new ArgumentException(
                $"Metric dimension key '{key}' is prohibited due to high cardinality rules (Manifest §8).",
                nameof(key));
        }
    }

    /// <summary>
    /// Validates a collection of tag key-value pairs.
    /// </summary>
    public static void ValidateTags(IEnumerable<KeyValuePair<string, object?>> tags)
    {
        VKGuard.NotNull(tags, nameof(tags));

        foreach (var tag in tags)
        {
            ValidateTag(tag.Key);
        }
    }
}
