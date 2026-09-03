using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.Common.Internal;

// [AP.01] sealed
internal sealed class DefaultPolicyRegistry : IVKPolicyRegistry
{
    private readonly ConcurrentDictionary<string, IVKResiliencePipeline> _pipelines = new();
    private readonly IServiceProvider _serviceProvider;

    public IReadOnlyCollection<string> RegisteredPipelineNames => _pipelines.Keys.ToList();

    public DefaultPolicyRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = VKGuard.NotNull(serviceProvider);
    }

    public IVKResiliencePipeline GetPipeline(string pipelineName)
    {
        VKGuard.NotNullOrWhiteSpace(pipelineName);

        if (_pipelines.TryGetValue(pipelineName, out var pipeline))
        {
            return pipeline;
        }

        throw new KeyNotFoundException($"Resilience pipeline '{pipelineName}' was not found in the registry.");
    }

    public bool TryGetPipeline(string pipelineName, out IVKResiliencePipeline? pipeline)
    {
        VKGuard.NotNullOrWhiteSpace(pipelineName);
        return _pipelines.TryGetValue(pipelineName, out pipeline);
    }

    public void RegisterPipeline(string pipelineName, IVKResiliencePipeline pipeline)
    {
        VKGuard.NotNullOrWhiteSpace(pipelineName);
        VKGuard.NotNull(pipeline);

        _pipelines.AddOrUpdate(pipelineName, pipeline, (_, _) => pipeline);
    }

    public IVKResiliencePipeline GetOrAddPipeline(
        string pipelineName,
        Func<IVKPolicyBuilder, IVKResiliencePipeline> configure)
    {
        VKGuard.NotNullOrWhiteSpace(pipelineName);
        VKGuard.NotNull(configure);

        return _pipelines.GetOrAdd(pipelineName, name =>
        {
            var builder = new DefaultPolicyBuilder(name, _serviceProvider);
            return configure(builder);
        });
    }
}
