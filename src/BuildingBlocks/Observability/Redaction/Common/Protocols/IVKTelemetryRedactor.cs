using System;
using VK.Blocks.Core;

namespace VK.Blocks.Observability;

/// <summary>
/// Unified redaction pipeline for all telemetry signals (Log, Trace, Metric).
/// Complies with Manifest §7 (PII Redaction Pipeline Reuse).
/// </summary>
// [AP.03] Level 1 Public API
public interface IVKTelemetryRedactor
{
    /// <summary>
    /// Determines whether the given field or tag key represents a sensitive field.
    /// </summary>
    bool IsSensitive(string key);

    /// <summary>
    /// Redacts a telemetry attribute or tag value based on its key.
    /// Returns the original value if no redaction is needed.
    /// </summary>
    object? Redact(string key, object? value);

    /// <summary>
    /// Redacts sensitive data from a JSON or structured text payload.
    /// </summary>
    string? RedactJson(string? json);

    /// <summary>
    /// Checks whether the specified type has any property marked as sensitive or redacted.
    /// </summary>
    bool HasSensitiveProperties(Type type);

    /// <summary>
    /// Gets the security level for a specific property on the given type.
    /// </summary>
    VKSecurityLevel GetSecurityLevel(Type type, string propertyName);
}
