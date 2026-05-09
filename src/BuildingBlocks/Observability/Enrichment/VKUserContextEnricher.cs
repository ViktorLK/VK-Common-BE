using System;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Observability;

public sealed class VKUserContextEnricher(IVKUserContext userContext, IOptions<VKObservabilityOptions> options) : IVKLogEnricher
{
    public void Enrich(Action<string, object?> propertyAdder)
    {
        if (userContext.IsAuthenticated)
        {
            propertyAdder("user.id", userContext.UserId);
            if (options.Value.IncludeUserName)
            {
                propertyAdder("user.name", userContext.UserName);
            }
        }
    }
}
