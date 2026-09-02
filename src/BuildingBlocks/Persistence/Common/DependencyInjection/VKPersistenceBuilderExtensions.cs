using VK.Blocks.Core;

namespace VK.Blocks.Persistence;

/// <summary>
/// Extension methods for customizing persistence building block registrations via <see cref="IVKPersistenceBuilder"/>.
/// </summary>
public static partial class VKPersistenceBuilderExtensions
{
    /// <summary>
    /// Overrides the default <see cref="IVKAuditProvider"/> implementation with a custom provider.
    /// </summary>
    /// <typeparam name="TProvider">The custom audit provider type.</typeparam>
    /// <param name="builder">The persistence builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKPersistenceBuilder OverrideAuditProvider<TProvider>(this IVKPersistenceBuilder builder)
        where TProvider : class, IVKAuditProvider
    {
        VKGuard.NotNull(builder);
        builder.WithScoped<VKPersistenceBlock, IVKAuditProvider, TProvider>();
        return builder;
    }
}
