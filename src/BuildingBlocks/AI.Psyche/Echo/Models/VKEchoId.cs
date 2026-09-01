using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Strongly-typed identifier for an Echo message entry.
/// Follows AP.01 and CS.06.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Strongly-typed identifier struct containing no executable business logic.")]
[VKStronglyTypedId]
public partial record struct VKEchoId;
