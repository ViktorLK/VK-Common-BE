using Xunit;

namespace VK.Blocks.Testing;

/// <summary>
/// A marker and definition class for xUnit Collection Fixtures in VK.Blocks ecosystem.
/// Use this class or derive from it to simplify creating shared test collections.
/// </summary>
/// <typeparam name="TFixture">The fixture type shared across the collection.</typeparam>
public abstract class VKTestCollectionDefinition<TFixture> : ICollectionFixture<TFixture>
    where TFixture : class, IVKTestFixture
{
}
