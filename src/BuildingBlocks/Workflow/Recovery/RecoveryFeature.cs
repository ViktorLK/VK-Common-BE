using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Workflow.Recovery.Internal;

namespace VK.Blocks.Workflow;

/// <summary>
/// Feature marker and registration for Workflow Recovery slice.
/// </summary>
[VKFeature(typeof(VKWorkflowBlock), OptionsType = typeof(VKRecoveryOptions))]
internal sealed partial class RecoveryFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKRecoveryOptions options)
    {
        services.TryAddSingleton<WorkflowOrphanScanJobQueue>();
        services.AddHostedService<DefaultWorkflowRecoveryBackgroundService>();
    }
}
