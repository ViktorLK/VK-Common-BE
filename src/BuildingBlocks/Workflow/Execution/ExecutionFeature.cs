using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Workflow.Execution.Internal;

namespace VK.Blocks.Workflow;

/// <summary>
/// Feature marker and registration for Workflow Execution slice.
/// </summary>
[VKFeature(typeof(VKWorkflowBlock), OptionsType = typeof(VKExecutionOptions))]
internal sealed partial class ExecutionFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKExecutionOptions options)
    {
        services.TryAddScoped<IVKWorkflowOrchestrator, DefaultWorkflowOrchestrator>();
    }
}
