using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// Represents validation failures.
/// </summary>
public sealed class VKValidationException : VKBusinessException
{
    /// <summary>
    /// Gets the validation errors list.
    /// </summary>
    public IReadOnlyList<VKErrorDetail> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKValidationException"/> class.
    /// </summary>
    public VKValidationException(string message, IReadOnlyList<VKErrorDetail> errors)
        : base("Validation.Error", message, VKErrorType.Validation)
    {
        Errors = VKGuard.NotNull(errors);
    }
}
