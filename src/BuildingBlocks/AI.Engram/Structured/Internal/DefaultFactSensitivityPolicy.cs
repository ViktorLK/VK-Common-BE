namespace VK.Blocks.AI.Engram.Structured.Internal;

/// <summary>
/// Default implementation of <see cref="IVKFactSensitivityPolicy"/>.
/// Follows AP.03 (internal sealed in Internal/ folder).
/// </summary>
internal sealed class DefaultFactSensitivityPolicy : IVKFactSensitivityPolicy
{
    public string MaskSensitiveValue(object? value)
    {
        if (value is null) return "***null***";
        string str = value.ToString() ?? string.Empty;
        if (str.Length <= 4) return "***";
        return str[..2] + "*****" + str[^2..];
    }
}
