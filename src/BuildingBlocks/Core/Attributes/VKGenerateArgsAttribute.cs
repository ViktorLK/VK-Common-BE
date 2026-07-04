using System;

namespace VK.Blocks.Core;

/// <summary>
/// When applied to an options class (IVKBlockOptions), triggers the source generator
/// to create a matching "Args" record with all properties as nullable for request-scoped overrides.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class VKGenerateArgsAttribute : Attribute;
