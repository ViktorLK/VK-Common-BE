using System;

namespace VK.Blocks.Validation;

/// <summary>
/// Attribute used to mark classes, records, structs, properties, or parameters for validation.
/// When applied to a class or record, the source generator will automatically generate a zero-reflection <see cref="IVKValidator{T}"/> implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class VKValidateAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the cascade mode for validation.
    /// Default is <see cref="VKCascadeMode.Continue"/>.
    /// </summary>
    public VKCascadeMode CascadeMode { get; init; } = VKCascadeMode.Continue;
}
