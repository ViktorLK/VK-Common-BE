using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Workflow.Idempotency.Internal;

namespace VK.Blocks.Workflow;

/// <summary>
/// Feature marker and registration for Workflow Idempotency slice.
/// </summary>
[VKFeature(typeof(VKWorkflowBlock), OptionsType = typeof(VKIdempotencyOptions))]
internal sealed partial class IdempotencyFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKIdempotencyOptions options)
    {
        services.TryAddSingleton<IVKIdempotencyKeyGenerator, DefaultIdempotencyKeyGenerator>();
    }
}
