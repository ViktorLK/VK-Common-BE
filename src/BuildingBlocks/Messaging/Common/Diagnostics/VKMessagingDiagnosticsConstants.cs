namespace VK.Blocks.Messaging;

/// <summary>
/// Diagnostic constants for the Messaging building block.
/// </summary>
public static class VKMessagingDiagnosticsConstants
{
    public const string DiagnosticNamespace = "VK.Blocks.Messaging";

    public static class Metrics
    {
        public const string PublishLatency = "messaging.publish.latency";
        public const string ConsumeLatency = "messaging.consume.latency";
        public const string QueueBacklog = "messaging.queue.backlog";
        public const string DlqCount = "messaging.dlq.count";
    }
}
