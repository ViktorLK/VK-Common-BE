using System;
using System.Collections.Generic;

namespace VK.Blocks.Core.Exceptions;

/// <summary>
/// Exception thrown when there is a configuration or registration error within the building block system.
/// </summary>
public sealed class VKDependencyException : VKBaseException
{
    private const string DefaultCode = "Core.DependencyError";

    /// <summary>
    /// Gets the identifier of the building block that is missing.
    /// </summary>
    public string? RequiredBlock => Extensions.TryGetValue(nameof(RequiredBlock), out var val) ? val as string : null;

    /// <summary>
    /// Gets the identifier of the building block that has the missing dependency.
    /// </summary>
    public string? DependentBlock => Extensions.TryGetValue(nameof(DependentBlock), out var val) ? val as string : null;

    /// <summary>
    /// Gets the name of the missing configuration section.
    /// </summary>
    public string? SectionName => Extensions.TryGetValue(nameof(SectionName), out var val) ? val as string : null;

    /// <summary>
    /// Gets the sequence of blocks forming a circular dependency.
    /// </summary>
    public string? CyclePath => Extensions.TryGetValue(nameof(CyclePath), out var val) ? val as string : null;

    /// <summary>
    /// Gets the first conflicting feature.
    /// </summary>
    public string? FeatureA => Extensions.TryGetValue(nameof(FeatureA), out var val) ? val as string : null;

    /// <summary>
    /// Gets the second conflicting feature.
    /// </summary>
    public string? FeatureB => Extensions.TryGetValue(nameof(FeatureB), out var val) ? val as string : null;

    /// <summary>
    /// Gets the name of the missing required option.
    /// </summary>
    public string? OptionName => Extensions.TryGetValue(nameof(OptionName), out var val) ? val as string : null;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKDependencyException"/> class.
    /// </summary>
    public VKDependencyException(string message, Exception? innerException = null)
        : base(DefaultCode, message, statusCode: 500, isPublic: false, innerException: innerException)
    {
    }

    /// <summary>
    /// Creates an exception for a missing building block dependency.
    /// </summary>
    public static VKDependencyException MissingDependency(string requiredBlock, string dependentBlock)
    {
        var message = $"VKBlock '{dependentBlock}' requires '{requiredBlock}' to be registered first. Please ensure the required block is added during startup.";

        return new VKDependencyException(message)
            .WithExtension(nameof(RequiredBlock), requiredBlock)
            .WithExtension(nameof(DependentBlock), dependentBlock);
    }

    /// <summary>
    /// Creates an exception for a missing configuration section.
    /// </summary>
    public static VKDependencyException MissingSection(string sectionName)
    {
        var message = $"Required configuration section '{sectionName}' is missing. Please check your appsettings.json.";

        return new VKDependencyException(message)
            .WithExtension(nameof(SectionName), sectionName);
    }

    /// <summary>
    /// Creates an exception for a circular dependency between building blocks.
    /// </summary>
    public static VKDependencyException CircularDependency(string blockId, IEnumerable<string> path)
    {
        var pathString = string.Join(" -> ", path);
        var message = $"Circular dependency detected at VKBlock '{blockId}'. Path: {pathString}";

        return new VKDependencyException(message)
            .WithExtension(nameof(CyclePath), pathString);
    }

    /// <summary>
    /// Creates an exception for conflicting features within a building block.
    /// </summary>
    public static VKDependencyException FeatureConflict(string featureA, string featureB, string? blockId = null)
    {
        var target = blockId is not null ? $" in VKBlock '{blockId}'" : string.Empty;
        var message = $"Features '{featureA}' and '{featureB}' cannot be enabled simultaneously{target}.";

        return new VKDependencyException(message)
            .WithExtension(nameof(FeatureA), featureA)
            .WithExtension(nameof(FeatureB), featureB);
    }

    /// <summary>
    /// Creates an exception for a missing required option value.
    /// </summary>
    public static VKDependencyException RequiredOptionMissing(string optionName, string reason)
    {
        var message = $"Required option '{optionName}' is missing. {reason}";

        return new VKDependencyException(message)
            .WithExtension(nameof(OptionName), optionName)
            .WithExtension("Reason", reason); // Reason does not have a property as it is part of the message
    }

    /// <summary>
    /// Creates an exception for when an options type is registered via IOptions but missing the direct singleton registration.
    /// Required for the VK.Blocks Dual-Registration pattern.
    /// </summary>
    public static VKDependencyException DualRegistrationMissing(string optionsType)
    {
        var message = $"Options type '{optionsType}' is already registered but no singleton instance is available. " +
                      "This usually happens if the options were registered via standard IOptions pipeline without the VK.Blocks Dual-Registration pattern.";

        return new VKDependencyException(message)
            .WithExtension("OptionsType", optionsType);
    }
}
