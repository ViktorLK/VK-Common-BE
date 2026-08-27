using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace VK.Blocks.Testing;

/// <summary>
/// Base class for class-level test fixtures (used with xUnit's <see cref="IClassFixture{TFixture}"/>).
/// Executes initialization and disposal once per test class execution.
/// </summary>
public abstract class VKClassFixture : IVKTestFixture
{
    private readonly List<Func<IServiceProvider, CancellationToken, Task>> _initActions = [];
    private readonly List<Func<IServiceProvider, CancellationToken, Task>> _cleanupActions = [];

    /// <inheritdoc />
    public virtual IServiceProvider Services { get; protected set; } = default!;

    /// <summary>
    /// Adds an asynchronous action to be executed once during class-level initialization.
    /// </summary>
    /// <param name="action">The initialization action.</param>
    /// <returns>This fixture instance for fluent chaining.</returns>
    public VKClassFixture OnInitialize(Func<IServiceProvider, CancellationToken, Task> action)
    {
        _initActions.Add(action);
        return this;
    }

    /// <summary>
    /// Adds an asynchronous action to be executed once during class-level cleanup/disposal.
    /// </summary>
    /// <param name="action">The cleanup action.</param>
    /// <returns>This fixture instance for fluent chaining.</returns>
    public VKClassFixture OnDispose(Func<IServiceProvider, CancellationToken, Task> action)
    {
        _cleanupActions.Add(action);
        return this;
    }

    /// <inheritdoc />
    public virtual async Task InitializeAsync()
    {
        if (Services is null)
        {
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();
            var builder = new VKTestFixtureBuilder(services, configuration);

            Configure(builder);

            Services = builder.Services.BuildServiceProvider();
        }

        await OnInitializeCoreAsync().ConfigureAwait(false);

        foreach (var action in _initActions)
        {
            await action(Services, CancellationToken.None).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public virtual async Task DisposeAsync()
    {
        foreach (var action in _cleanupActions)
        {
            await action(Services, CancellationToken.None).ConfigureAwait(false);
        }

        await OnDisposeCoreAsync().ConfigureAwait(false);

        if (Services is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Configures services and configuration via <see cref="IVKTestFixtureBuilder"/>.
    /// </summary>
    /// <param name="builder">The test fixture builder.</param>
    protected virtual void Configure(IVKTestFixtureBuilder builder)
    {
    }

    /// <summary>
    /// Creates the initial test configuration for this fixture.
    /// </summary>
    /// <returns>The created configuration instance.</returns>
    protected virtual IConfiguration CreateConfiguration() => VKEmptyConfiguration.Instance;

    /// <summary>
    /// Core initialization hook called before registered initialization actions.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnInitializeCoreAsync() => Task.CompletedTask;

    /// <summary>
    /// Core cleanup hook called before service provider disposal.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnDisposeCoreAsync() => Task.CompletedTask;
}
