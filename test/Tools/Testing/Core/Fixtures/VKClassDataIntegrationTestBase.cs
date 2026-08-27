using Xunit;

namespace VK.Blocks.Testing;

/// <summary>
/// Convenient base class for integration tests that bind a class-level dataset with automatic setup/cleanup.
/// </summary>
/// <typeparam name="TFixture">The class fixture type derived from <see cref="VKClassDataFixture{TData}"/>.</typeparam>
/// <typeparam name="TData">The test data contract provider type.</typeparam>
public abstract class VKIntegrationTestBase<TFixture, TData> : VKIntegrationTestBase<TFixture>, IClassFixture<TFixture>
    where TFixture : VKClassDataFixture<TData>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKIntegrationTestBase{TFixture, TData}"/> class.
    /// </summary>
    /// <param name="fixture">The shared class fixture instance.</param>
    protected VKIntegrationTestBase(TFixture fixture) : base(fixture)
    {
    }
}

/// <summary>
/// Convenient all-in-one base class where the test class itself acts as the test dataset provider.
/// </summary>
/// <typeparam name="TSelf">The concrete test class type.</typeparam>
public abstract class VKClassDataIntegrationTestBase<TSelf> : VKIntegrationTestBase<VKClassDataFixture<TSelf>, TSelf>
    where TSelf : VKClassDataIntegrationTestBase<TSelf>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKClassDataIntegrationTestBase{TSelf}"/> class.
    /// </summary>
    /// <param name="fixture">The shared class fixture instance.</param>
    protected VKClassDataIntegrationTestBase(VKClassDataFixture<TSelf> fixture) : base(fixture)
    {
    }
}
