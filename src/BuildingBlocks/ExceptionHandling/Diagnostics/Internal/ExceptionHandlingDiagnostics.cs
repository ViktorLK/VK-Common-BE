using VK.Blocks.Core;

namespace VK.Blocks.ExceptionHandling.Diagnostics.Internal;

/// <summary>
/// Diagnostics for the ExceptionHandling building block.
/// This will auto-generate ActivitySource and Meter using [VKBlockDiagnostics] source generator.
/// </summary>
[VKBlockDiagnostics<VKExceptionHandlingBlock>]
internal static partial class ExceptionHandlingDiagnostics;
