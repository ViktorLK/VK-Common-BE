using VK.Blocks.Core.Contracts;

namespace VK.Blocks.Core.Diagnostics;

/// <summary>
/// Diagnostics for the Core building block.
/// This will auto-generate ActivitySource and Meter using [VKBlockDiagnostics] source generator.
/// </summary>
[VKBlockDiagnostics<CoreBlock>]
internal static partial class CoreDiagnostics;
