using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Workflow.Persistence.Internal;

namespace VK.Blocks.Workflow;

/// <summary>
/// Feature marker and registration for Workflow Persistence slice.
/// </summary>
[VKFeature(typeof(VKWorkflowBlock), OptionsType = typeof(VKPersistenceOptions))]
internal sealed partial class PersistenceFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKPersistenceOptions options)
    {
        services.TryAddSingleton<IVKWorkflowStore, InMemoryWorkflowStore>();
    }
}
