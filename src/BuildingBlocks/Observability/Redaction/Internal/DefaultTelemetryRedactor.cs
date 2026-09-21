using System;
using System.Collections.Frozen;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Observability.Redaction.Internal;

/// <summary>
/// Default implementation of <see cref="IVKTelemetryRedactor"/> using configured sensitive field names
/// and <see cref="VKPropertySecurityCache"/>.
/// Complies with Manifest §7.
/// </summary>
// [AP.01] sealed
// [AP.03] Internal scoping without VK prefix
internal sealed class DefaultTelemetryRedactor : IVKTelemetryRedactor
{
    private const string RedactedContent = "[REDACTED]";
    private readonly FrozenSet<string> _sensitiveFields;
    private readonly Regex? _redactPattern;

    public DefaultTelemetryRedactor(IOptions<VKObservabilityOptions> options)
    {
        VKGuard.NotNull(options, nameof(options));

        var fields = options.Value.SensitiveFieldNames ?? [];
        _sensitiveFields = fields.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        _redactPattern = BuildPattern(fields);
    }

    /// <inheritdoc />
    public bool IsSensitive(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return _sensitiveFields.Contains(key);
    }

    /// <inheritdoc />
    public object? Redact(string key, object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (IsSensitive(key))
        {
            return RedactedContent;
        }

        if (value is VKSensitiveString)
        {
            return RedactedContent;
        }

        return value;
    }

    /// <inheritdoc />
    public string? RedactJson(string? json)
    {
        if (string.IsNullOrEmpty(json) || _redactPattern is null || _sensitiveFields.Count == 0)
        {
            return json;
        }

        return _redactPattern.Replace(json, $"$1\"{RedactedContent}\"");
    }

    /// <inheritdoc />
    public bool HasSensitiveProperties(Type type)
    {
        VKGuard.NotNull(type, nameof(type));
        return VKPropertySecurityCache.HasSensitiveProperties(type);
    }

    /// <inheritdoc />
    public VKSecurityLevel GetSecurityLevel(Type type, string propertyName)
    {
        VKGuard.NotNull(type, nameof(type));
        VKGuard.NotNullOrWhiteSpace(propertyName, nameof(propertyName));
        return VKPropertySecurityCache.GetLevel(type, propertyName);
    }

    private static Regex? BuildPattern(string[] fields)
    {
        if (fields.Length == 0)
        {
            return null;
        }

        var escapedFields = fields.Select(Regex.Escape);
        var joined = string.Join("|", escapedFields);

        // Matches JSON key-value pattern: "key"\s*:\s*value
        var pattern = $@"(""(?:{joined})""\s*:\s*)(?:""[^""]*""|-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?|true|false|null)";

        return new Regex(
            pattern,
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));
    }
}
