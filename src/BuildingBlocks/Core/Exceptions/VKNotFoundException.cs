using System;

namespace VK.Blocks.Core.Exceptions;

/// <summary>
/// Exception thrown when a requested resource or entity is not found.
/// Maps to HTTP 404 Not Found.
/// </summary>
public sealed class VKNotFoundException : VKBaseException
{
    private const string DefaultCode = "Core.NotFound";

    /// <summary>
    /// Gets the type of the entity that was not found.
    /// </summary>
    public string? EntityType => Extensions.TryGetValue(nameof(EntityType), out var val) ? val as string : null;

    /// <summary>
    /// Gets the identifier of the entity that was not found.
    /// </summary>
    public string? EntityId => Extensions.TryGetValue(nameof(EntityId), out var val) ? val as string : null;

    public VKNotFoundException(string message, Exception? innerException = null)
        : base(DefaultCode, message, statusCode: 404, isPublic: true, innerException: innerException)
    {
    }

    public static VKNotFoundException ForEntity(string entityType, string entityId)
    {
        return new VKNotFoundException($"Entity '{entityType}' with ID '{entityId}' was not found.")
            .WithExtension(nameof(EntityType), entityType)
            .WithExtension(nameof(EntityId), entityId);
    }
}
