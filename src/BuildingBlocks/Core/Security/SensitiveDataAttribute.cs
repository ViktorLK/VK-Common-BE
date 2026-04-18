using System;

namespace VK.Blocks.Core.Security;

/// <summary>
/// Marks a property as containing sensitive information (PII, secrets, etc.)
/// that should be masked or redacted in logs and observability telemetry.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class SensitiveDataAttribute : Attribute;

