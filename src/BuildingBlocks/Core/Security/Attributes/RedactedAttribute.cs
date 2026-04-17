using System;

namespace VK.Blocks.Core.Security.Attributes;

/// <summary>
/// Marks a property to be completely redacted (hidden) from logs, 
/// typically for extremely sensitive data like passwords or private keys.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class RedactedAttribute : Attribute;
