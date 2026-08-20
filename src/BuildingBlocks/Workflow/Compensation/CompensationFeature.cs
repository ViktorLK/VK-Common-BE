using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Workflow.Compensation.Internal;

namespace VK.Blocks.Workflow;

/// <summary>
/// Feature marker and registration for Workflow Compensation slice.
/// </summary>
[VKFeature(typeof(VKWorkflowBlock), OptionsType = typeof(VKCompensationOptions))]
internal sealed partial class CompensationFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKCompensationOptions options)
    {
        services.TryAddScoped<DefaultWorkflowCompensationExecutor>();
    }
}
