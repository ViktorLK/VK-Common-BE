using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy.Diagnostics.Internal;

/// <summary>
/// Provides centralized diagnostics and telemetry for the MultiTenancy core block.
/// </summary>
[VKBlockDiagnostics<VKMultiTenancyBlock>]
internal static partial class MultiTenancyDiagnostics
{
    // ActivitySource and Meter are generated automatically.
}
