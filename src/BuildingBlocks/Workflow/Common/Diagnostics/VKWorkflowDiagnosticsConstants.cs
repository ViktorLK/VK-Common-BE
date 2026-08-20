namespace VK.Blocks.Workflow;

/// <summary>
/// Diagnostic tokens and activity constants for the Workflow block.
/// </summary>
public static class VKWorkflowDiagnosticsConstants
{
    /// <summary>
    /// Telemetry meter and activity source name.
    /// </summary>
    public const string SourceName = "VK.Blocks.Workflow";

    /// <summary>
    /// Telemetry meter name.
    /// </summary>
    public const string MeterName = "VK.Blocks.Workflow";

    public static class Metrics
    {
        public const string InstancesCreated = "vk.workflow.instances.created";
        public const string InstancesCompleted = "vk.workflow.instances.completed";
        public const string InstancesFailed = "vk.workflow.instances.failed";
        public const string CompensationsTriggered = "vk.workflow.compensations.triggered";
        public const string OrphansDetected = "vk.workflow.orphans.detected";
        public const string EndToEndDuration = "vk.workflow.duration.seconds";
        public const string ExternalCallDuration = "vk.workflow.external.duration.seconds";
    }
}
