
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        Action<TOptions> configure)
        where TOptions : class, new()
    {
        var options = new TOptions();
        configure(options);
        services.Configure(configure);
        services.AddSingleton(options);
        return options;
    }
}
