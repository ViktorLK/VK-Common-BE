using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Testing.AspNetCore;

/// <summary>
/// Fixture wrapping <see cref="WebApplicationFactory{TProgram}"/> with VK-standard configuration overrides,
/// service replacement, and HttpClient creation.
/// </summary>
/// <typeparam name="TProgram">The program entry point type from the target web application.</typeparam>
public class VKWebApplicationFixture<TProgram> : IVKTestFixture
    where TProgram : class
{
    private WebApplicationFactory<TProgram>? _factory;
    private readonly List<IVKServiceOverride> _overrides = [];
    private readonly List<Action<IWebHostBuilder>> _webHostConfigurations = [];

    /// <inheritdoc />
    public IServiceProvider Services => _factory?.Services
        ?? throw new InvalidOperationException("Fixture has not been initialized. Ensure InitializeAsync is called.");

    /// <summary>
    /// Creates a default HttpClient instance.
    /// </summary>
    /// <returns>A configured <see cref="HttpClient"/>.</returns>
    public HttpClient CreateClient() => _factory!.CreateClient();

    /// <summary>
    /// Creates an HttpClient instance with specified client options.
    /// </summary>
    /// <param name="options">The client options.</param>
    /// <returns>A configured <see cref="HttpClient"/>.</returns>
    public HttpClient CreateClient(WebApplicationFactoryClientOptions options)
        => _factory!.CreateClient(options);

    /// <summary>
    /// Registers a service override. Must be called prior to fixture initialization.
    /// </summary>
    /// <param name="serviceOverride">The service override contract.</param>
    /// <returns>The fixture instance for chaining.</returns>
    public VKWebApplicationFixture<TProgram> WithServiceOverride(IVKServiceOverride serviceOverride)
    {
        _overrides.Add(serviceOverride);
        return this;
    }

    /// <summary>
    /// Adds custom web host configuration actions.
    /// </summary>
    /// <param name="configure">Configuration delegate.</param>
    /// <returns>The fixture instance for chaining.</returns>
    public VKWebApplicationFixture<TProgram> ConfigureWebHost(Action<IWebHostBuilder> configure)
    {
        _webHostConfigurations.Add(configure);
        return this;
    }

    /// <summary>
    /// Overrides configuration values via in-memory collection.
    /// </summary>
    /// <param name="overrides">The key-value configuration dictionary.</param>
    /// <returns>The fixture instance for chaining.</returns>
    public VKWebApplicationFixture<TProgram> WithConfiguration(Dictionary<string, string?> overrides)
    {
        _webHostConfigurations.Add(builder =>
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(overrides)));
        return this;
    }

    /// <inheritdoc />
    public Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<TProgram>()
            .WithWebHostBuilder(builder =>
            {
                foreach (var config in _webHostConfigurations)
                {
                    config(builder);
                }

                builder.ConfigureTestServices(services =>
                {
                    foreach (var svcOverride in _overrides)
                    {
                        svcOverride.Apply(services);
                    }
                });
            });

        // Eager initialization of host
        _ = _factory.Server;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync().ConfigureAwait(false);
        }
    }
}
