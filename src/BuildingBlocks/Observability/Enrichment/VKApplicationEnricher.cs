using System;
using Microsoft.Extensions.Options;
namespace VK.Blocks.Observability;

public sealed class VKApplicationEnricher(IOptions<VKObservabilityOptions> options) : IVKLogEnricher
{
    public void Enrich(Action<string, object?> propertyAdder)
    {
        var opt = options.Value;
        propertyAdder("service.name", opt.ApplicationName);
        propertyAdder("service.version", opt.ServiceVersion);
        propertyAdder("deployment.environment", opt.Environment);
    }
}
