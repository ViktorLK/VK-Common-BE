using VK.Blocks.Core;

namespace VK.Blocks.Messaging;

/// <summary>
/// Domain-specific error constants for the Messaging building block.
/// Follows CS.01: {ModuleName}.{Category}.{Reason} format.
/// </summary>
public static class VKMessagingErrors
{
    /// <summary>
    /// Outbox pattern errors.
    /// </summary>
    public static class Outbox
    {
        public static readonly VKError WriteFailed = VKError.Failure(
            "Messaging.Outbox.WriteFailed",
            "Failed to write message to the outbox store.");

        public static readonly VKError DispatchFailed = VKError.Failure(
            "Messaging.Outbox.DispatchFailed",
            "Failed to dispatch pending outbox message.");

        public static readonly VKError MessageNotFound = VKError.NotFound(
            "Messaging.Outbox.MessageNotFound",
            "The specified outbox message was not found.");
    }

    /// <summary>
    /// Event Sourcing errors.
    /// </summary>
    public static class EventStore
    {
        public static readonly VKError ReadFailed = VKError.Failure(
            "Messaging.EventStore.ReadFailed",
            "Failed to read events from the event store.");

        public static readonly VKError WriteFailed = VKError.Failure(
            "Messaging.EventStore.WriteFailed",
            "Failed to append events to the event store.");

        public static readonly VKError ConcurrencyConflict = VKError.Conflict(
            "Messaging.EventStore.ConcurrencyConflict",
            "Event store stream concurrency conflict detected.");
    }

    /// <summary>
    /// Message Bus errors.
    /// </summary>
    public static class Bus
    {
        public static readonly VKError PublishFailed = VKError.Failure(
            "Messaging.Bus.PublishFailed",
            "Failed to publish message to the message bus.");

        public static readonly VKError SendFailed = VKError.Failure(
            "Messaging.Bus.SendFailed",
            "Failed to send message to the specified endpoint.");

        public static readonly VKError ConsumerFailed = VKError.Failure(
            "Messaging.Bus.ConsumerFailed",
            "An error occurred while consuming message.");
    }

    /// <summary>
    /// Serialization errors.
    /// </summary>
    public static class Serialization
    {
        public static readonly VKError SerializationFailed = VKError.Failure(
            "Messaging.Serialization.SerializationFailed",
            "Failed to serialize message payload.");

        public static readonly VKError DeserializationFailed = VKError.Failure(
            "Messaging.Serialization.DeserializationFailed",
            "Failed to deserialize message payload.");
    }
}
