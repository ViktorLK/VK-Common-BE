using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Labs.PersonaWeavePulsar.DependencyInjection;

internal sealed class PwpBuilder(IServiceCollection services, IConfiguration configuration) : IPwpBuilder
{
    public IServiceCollection Services { get; } = services;
    public IConfiguration Configuration { get; } = configuration;
}

public interface IPwpBuilder
{
    IServiceCollection Services { get; }
    IConfiguration Configuration { get; }
}
