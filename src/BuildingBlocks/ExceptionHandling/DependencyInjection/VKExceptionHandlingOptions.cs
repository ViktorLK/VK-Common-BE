using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// Configuration options for the ExceptionHandling building block.
/// </summary>
public sealed record VKExceptionHandlingOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:ExceptionHandling";

    /// <summary>
    /// Gets or sets a value indicating whether this block is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include full exception details in the error response.
    /// Should be false in production environments for security.
    /// </summary>
    public bool IncludeExceptionDetails { get; init; } = false;

    /// <summary>
    /// Gets or sets the list of exception types that should be categorized as validation errors.
    /// </summary>
    public IList<Type> ValidationExceptionTypes { get; init; } = [];
}
