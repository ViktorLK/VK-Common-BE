using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace VK.Blocks.Workflow.Common.Diagnostics.Internal;

/// <summary>
/// OpenTelemetry metrics recorder for the Workflow building block.
/// </summary>
internal sealed class DefaultWorkflowMetrics : IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _instancesCreatedCounter;
    private readonly Counter<long> _instancesCompletedCounter;
    private readonly Counter<long> _instancesFailedCounter;
    private readonly Counter<long> _compensationsTriggeredCounter;
    private readonly Counter<long> _orphansDetectedCounter;
    private readonly Histogram<double> _endToEndDurationHistogram;
    private readonly Histogram<double> _externalCallDurationHistogram;

    public DefaultWorkflowMetrics()
    {
        _meter = new Meter(VKWorkflowDiagnosticsConstants.MeterName, "1.0.0");
        _instancesCreatedCounter = _meter.CreateCounter<long>(
            VKWorkflowDiagnosticsConstants.Metrics.InstancesCreated,
            description: "Number of Workflow instances created.");
        _instancesCompletedCounter = _meter.CreateCounter<long>(
            VKWorkflowDiagnosticsConstants.Metrics.InstancesCompleted,
            description: "Number of Workflow instances completed successfully.");
        _instancesFailedCounter = _meter.CreateCounter<long>(
            VKWorkflowDiagnosticsConstants.Metrics.InstancesFailed,
            description: "Number of Workflow instances failed.");
        _compensationsTriggeredCounter = _meter.CreateCounter<long>(
            VKWorkflowDiagnosticsConstants.Metrics.CompensationsTriggered,
            description: "Number of compensations triggered.");
        _orphansDetectedCounter = _meter.CreateCounter<long>(
            VKWorkflowDiagnosticsConstants.Metrics.OrphansDetected,
            description: "Number of orphan Workflow instances detected.");
        _endToEndDurationHistogram = _meter.CreateHistogram<double>(
            VKWorkflowDiagnosticsConstants.Metrics.EndToEndDuration,
            unit: "s",
            description: "End-to-end execution duration in seconds.");
        _externalCallDurationHistogram = _meter.CreateHistogram<double>(
            VKWorkflowDiagnosticsConstants.Metrics.ExternalCallDuration,
            unit: "s",
            description: "External call execution duration in seconds.");
    }

    public void RecordInstanceCreated(string workflowName)
    {
        _instancesCreatedCounter.Add(1, new KeyValuePair<string, object?>("workflow.name", workflowName));
    }

    public void RecordInstanceCompleted(string workflowName, double durationSeconds)
    {
        _instancesCompletedCounter.Add(1, new KeyValuePair<string, object?>("workflow.name", workflowName));
        _endToEndDurationHistogram.Record(durationSeconds, new KeyValuePair<string, object?>("workflow.name", workflowName));
    }

    public void RecordInstanceFailed(string workflowName, string errorType, double durationSeconds)
    {
        _instancesFailedCounter.Add(1,
            new KeyValuePair<string, object?>("workflow.name", workflowName),
            new KeyValuePair<string, object?>("error.type", errorType));
        _endToEndDurationHistogram.Record(durationSeconds,
            new KeyValuePair<string, object?>("workflow.name", workflowName),
            new KeyValuePair<string, object?>("error.type", errorType));
    }

    public void RecordCompensationTriggered(string workflowName)
    {
        _compensationsTriggeredCounter.Add(1, new KeyValuePair<string, object?>("workflow.name", workflowName));
    }

    public void RecordOrphanDetected(string workflowName, int count = 1)
    {
        _orphansDetectedCounter.Add(count, new KeyValuePair<string, object?>("workflow.name", workflowName));
    }

    public void RecordExternalCallDuration(string workflowName, double durationSeconds, bool success)
    {
        _externalCallDurationHistogram.Record(durationSeconds,
            new KeyValuePair<string, object?>("workflow.name", workflowName),
            new KeyValuePair<string, object?>("success", success));
    }

    public void Dispose()
    {
        _meter.Dispose();
    }
}
