namespace VK.Blocks.Core.Diagnostics.Internal;

/// <summary>
/// Diagnostics for the Core building block.
/// This will auto-generate ActivitySource and Meter using [VKBlockDiagnostics] source generator.
/// </summary>
[VKBlockDiagnostics<VKCoreBlock>]
internal static partial class CoreDiagnostics;
