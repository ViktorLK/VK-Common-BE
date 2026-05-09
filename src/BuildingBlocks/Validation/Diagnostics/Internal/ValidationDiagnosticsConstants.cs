namespace VK.Blocks.Validation.Diagnostics.Internal;

/// <summary>
/// Internal constants for the Validation diagnostics feature.
/// </summary>
internal static class ValidationDiagnosticsConstants
{
    /// <summary>
    /// The diagnostic source name for the Validation block.
    /// </summary>
    internal static readonly string SourceName = VKValidationBlock.Instance.ActivitySourceName;

    /// <summary>
    /// The meter name for the Validation block.
    /// </summary>
    internal static readonly string MeterName = VKValidationBlock.Instance.ActivitySourceName;
}
