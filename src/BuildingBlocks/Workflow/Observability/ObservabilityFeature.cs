using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Workflow.Common.Diagnostics.Internal;
using VK.Blocks.Workflow.Observability.Internal;

namespace VK.Blocks.Workflow;

/// <summary>
/// Feature marker and registration for Workflow Observability slice.
/// </summary>
[VKFeature(typeof(VKWorkflowBlock), OptionsType = typeof(VKObservabilityOptions))]
internal sealed partial class ObservabilityFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKObservabilityOptions options)
    {
        services.TryAddSingleton<DefaultWorkflowMetrics>();
        services.TryAddSingleton<IVKWorkflowAlertHandler, DefaultNoOpWorkflowAlertHandler>();
    }
}
