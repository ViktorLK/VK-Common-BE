namespace VK.Blocks.Validation.Attributes;

/// <summary>
/// Attribute used to mark method parameters or classes for automatic validation.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ValidateAttribute : Attribute
{
}
