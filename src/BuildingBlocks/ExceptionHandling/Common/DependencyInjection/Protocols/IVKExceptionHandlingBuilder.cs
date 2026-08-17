using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.ExceptionHandling.Common.DependencyInjection.Protocols;

/// <summary>
/// Defines the builder interface for VKExceptionHandlingBlock.
/// </summary>
public partial interface IVKExceptionHandlingBuilder : IVKBlockBuilder<VKExceptionHandlingBlock>;

/// <summary>
/// Default implementation of the builder interface for VKExceptionHandlingBlock.
/// </summary>
public sealed partial class VKExceptionHandlingBuilder : VKBlockBuilder<VKExceptionHandlingBlock>, IVKExceptionHandlingBuilder
{
    public VKExceptionHandlingBuilder(IServiceCollection services, IConfiguration configuration)
        : base(services, configuration)
    {
    }
}
