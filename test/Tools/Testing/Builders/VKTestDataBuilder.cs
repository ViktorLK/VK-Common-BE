using Bogus;

namespace VK.Blocks.Testing.Builders;

/// <summary>
/// Fluent builder for constructing test data objects with sensible defaults.
/// Uses Bogus for randomized default value generation.
/// </summary>
/// <typeparam name="T">The type to build.</typeparam>
public abstract class VKTestDataBuilder<T> where T : class
{
    private readonly List<Action<T>> _customizations = [];

    /// <summary>
    /// Gets the underlying Faker instance for random value generation in derived builders.
    /// </summary>
    protected Faker Faker { get; } = new();

    /// <summary>
    /// Apply a customization to the built object instance.
    /// </summary>
    /// <param name="customization">The customization delegate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public VKTestDataBuilder<T> With(Action<T> customization)
    {
        _customizations.Add(customization);
        return this;
    }

    /// <summary>
    /// Builds a single instance of <typeparamref name="T"/> with defaults and customizations applied.
    /// </summary>
    /// <returns>A new instance of <typeparamref name="T"/>.</returns>
    public T Build()
    {
        var instance = CreateDefault();
        foreach (var customization in _customizations)
        {
            customization(instance);
        }
        return instance;
    }

    /// <summary>
    /// Builds multiple instances of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="count">The number of instances to generate.</param>
    /// <returns>A list of generated instances.</returns>
    public IReadOnlyList<T> Build(int count)
        => Enumerable.Range(0, count).Select(_ => Build()).ToList();

    /// <summary>
    /// Override to provide the default instance populated with sensible baseline data.
    /// </summary>
    /// <returns>A default instance of <typeparamref name="T"/>.</returns>
    protected abstract T CreateDefault();
}
