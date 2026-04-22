using System;
using System.Collections.Generic;

namespace VK.Blocks.Core.Exceptions;

/// <summary>
/// Exception thrown when one or more validation failures occur.
/// Maps to HTTP 400 Bad Request.
/// </summary>
public sealed class VKValidationException : VKBaseException
{
    private const string DefaultCode = "Core.ValidationError";

    /// <summary>
    /// Gets the dictionary of validation errors, grouped by property name.
    /// </summary>
    public IReadOnlyDictionary<string, string[]>? Errors =>
        Extensions.TryGetValue(nameof(Errors), out var val) ? val as IReadOnlyDictionary<string, string[]> : null;

    public VKValidationException(string message, Exception? innerException = null)
        : base(DefaultCode, message, statusCode: 400, isPublic: true, innerException: innerException)
    {
    }

    public VKValidationException(IReadOnlyDictionary<string, string[]> errors)
        : this("One or more validation errors occurred.")
    {
        this.WithExtension(nameof(Errors), errors);
    }
}
