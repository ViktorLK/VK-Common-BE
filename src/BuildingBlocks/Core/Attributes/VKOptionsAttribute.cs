using System;

namespace VK.Blocks.Core;

/// <summary>
/// Controls code generation behavior and schema export for building block options.
/// Decorate options records/classes with this attribute to configure Args generation,
/// effective options merge extension methods, and compilation-time JSON schema export.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class VKOptionsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the request-level argument record generation mode. Default is <see cref="VKArgsGenerationMode.Explicit"/>.
    /// </summary>
    public VKArgsGenerationMode ArgsMode { get; set; } = VKArgsGenerationMode.Explicit;

    /// <summary>
    /// Gets or sets a value indicating whether to generate high-performance <c>ApplyArgs</c> merge extension methods. Default is <c>true</c>.
    /// </summary>
    public bool GenerateMerge { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to export JSON Schema for IDE appsettings autocompletion during compilation. Default is <c>false</c>.
    /// </summary>
    public bool GenerateJsonSchema { get; set; } = false;

    /// <summary>
    /// Gets or sets the base interface type that the generated Args record should implement (e.g., typeof(IVKAIArgs)).
    /// </summary>
    public Type? ArgsBaseType { get; set; }
}
