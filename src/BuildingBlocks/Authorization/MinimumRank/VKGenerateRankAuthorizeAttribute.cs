using System;

namespace VK.Blocks.Authorization;

/// <summary>
/// Triggers the automatic generation of typed authorization attributes for the decorated rank enum.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public sealed class VKGenerateRankAuthorizeAttribute : Attribute;
