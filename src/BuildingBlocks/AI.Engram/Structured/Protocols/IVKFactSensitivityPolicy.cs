namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy interface for masking sensitive structured fact values in diagnostics.
/// </summary>
public interface IVKFactSensitivityPolicy
{
    /// <summary>
    /// Masks sensitive object value for logging or telemetry display.
    /// </summary>
    string MaskSensitiveValue(object? value);
}
