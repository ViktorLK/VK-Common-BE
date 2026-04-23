using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.Core.Diagnostics.Internal;

/// <summary>
/// Diagnostics for the Core building block.
/// This will auto-generate ActivitySource and Meter using [VKBlockDiagnostics] source generator.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source generated diagnostics attributes do not contain testable logic.")]
[VKBlockDiagnostics<VKCoreBlock>]
internal static partial class CoreDiagnostics;
