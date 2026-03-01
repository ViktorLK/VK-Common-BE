namespace VK.Blocks.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class VKBlockDiagnosticsAttribute(string blockName) : Attribute
{
    public string BlockName { get; } = blockName;
    public string Version { get; init; } = "1.0.0";
}
