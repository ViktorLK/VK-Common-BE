using System;

namespace VK.Blocks.Core;

/// <summary>
/// Marks a class as diagnosable, triggering Rule 6 (Observability) compliance checks.
/// Used to opt-in any building block service or handler into the standard observability pattern.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class VKDiagnosableAttribute : Attribute;
