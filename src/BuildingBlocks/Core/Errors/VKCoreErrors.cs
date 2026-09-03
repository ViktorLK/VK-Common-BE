namespace VK.Blocks.Core;

/// <summary>
/// Domain-specific error constants for the Core building block.
/// Follows CS.01: {ModuleName}.{Category}.{Reason} format.
/// </summary>
public static class VKCoreErrors
{
    public static class Serialization
    {
        public static readonly VKError SerializationFailed = VKError.Failure(
            "Core.Serialization.SerializationFailed",
            "Failed to serialize object payload.");

        public static readonly VKError DeserializationFailed = VKError.Failure(
            "Core.Serialization.DeserializationFailed",
            "Failed to deserialize payload to target type.");

        public static readonly VKError PayloadTooLarge = VKError.Validation(
            "Core.Serialization.PayloadTooLarge",
            "The payload size exceeds the maximum allowed limit.");
    }

    public static class Infrastructure
    {
        public static readonly VKError GeneralFailure = VKError.Failure(
            "Core.Infrastructure.GeneralFailure",
            "An unexpected infrastructure failure occurred.");

        public static readonly VKError ProviderUnavailable = VKError.Failure(
            "Core.Infrastructure.ProviderUnavailable",
            "The requested external service provider is unavailable.");

        public static readonly VKError OperationCancelled = VKError.Failure(
            "Core.Infrastructure.OperationCancelled",
            "The operation was cancelled.");
    }

    public static class Concurrency
    {
        public static readonly VKError ConcurrentUpdate = VKError.Conflict(
            "Core.Concurrency.ConcurrentUpdate",
            "The entity has been modified by another process.");

        public static readonly VKError InvalidTokenFormat = VKError.Validation(
            "Core.Concurrency.InvalidTokenFormat",
            "The provided concurrency token is not a valid base64 string.");
    }
}
