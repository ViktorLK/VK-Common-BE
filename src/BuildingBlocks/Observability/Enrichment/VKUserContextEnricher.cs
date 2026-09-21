using System;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Observability;

public sealed class VKUserContextEnricher(IVKUserCoordinate userContext, IOptions<VKObservabilityOptions> options) : IVKLogEnricher
{
    private readonly VKObservabilityOptions _options = options.Value;

    public void Enrich(Action<string, object?> propertyAdder)
    {
        if (userContext.UserId != VKUserId.Anonymous)
        {
            propertyAdder("user.id", userContext.UserId.ToString());
        }
    }
}
