using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.Diagnostics.Internal;

/// <summary>
/// Partial class for resilience diagnostics implementation.
/// </summary>
[VKBlockDiagnostics<VKResilienceBlock>]
internal static partial class ResilienceDiagnostics
{
    private static readonly Counter<long> _strategyExecutions;

    static ResilienceDiagnostics()
    {
        _strategyExecutions = Meter.CreateCounter<long>(
            VKResilienceDiagnosticsConstants.StrategyExecutionCount,
            "count",
            "Total number of resilience strategy executions");
    }

    internal static Activity? StartActivity(string name) => Source.StartActivity(name);

    internal static void RecordStrategyExecution(string strategyName, bool success)
    {
        _strategyExecutions.Add(1, new TagList
        {
            { "strategy", strategyName },
            { "success", success }
        });
    }
}
