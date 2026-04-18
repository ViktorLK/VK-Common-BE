using System;
using System.Collections.Generic;

namespace VK.Blocks.Core.Exceptions;

/// <summary>
/// Abstract base for all application exceptions.
/// Provides a machine-readable <see cref="Code"/> alongside the human-readable message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BaseException"/> class.
/// </remarks>
/// <param name="code">The machine-readable identifier for the error.</param>
/// <param name="message">The human-readable message describing the error.</param>
/// <param name="statusCode">The suggested HTTP status code for this error.</param>
/// <param name="isPublic">Indicates whether the error message is safe to expose to clients.</param>
public abstract class BaseException(
    string code,
    string message,
    int statusCode = 400,
    bool isPublic = true,
    Exception? innerException = null) : Exception(message, innerException)
{
    private readonly Dictionary<string, object?> _extensions = [];

    /// <summary>
    /// Gets the unique error code defined by the domain.
    /// </summary>
    public string Code { get; } = code ?? throw new ArgumentNullException(nameof(code));

    /// <summary>
    /// Gets the suggested HTTP status code.
    /// </summary>
    public int StatusCode { get; } = statusCode;

    /// <summary>
    /// Gets a value indicating whether the error is safe to reveal to external clients.
    /// </summary>
    public bool IsPublic { get; } = isPublic;

    /// <summary>
    /// Gets additional diagnostic metadata associated with the exception.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Extensions => _extensions;

    /// <summary>
    /// Sets an extension value for diagnostic purposes.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    internal void SetExtension(string key, object? value) => _extensions[key] = value;
}
