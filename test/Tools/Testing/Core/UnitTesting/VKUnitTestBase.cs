using Moq;

namespace VK.Blocks.Testing;

/// <summary>
/// Lightweight Base class for Unit Tests supporting System Under Test (SUT) instantiation and Mock management.
/// </summary>
/// <typeparam name="TSut">The type of the System Under Test.</typeparam>
public abstract class VKUnitTestBase<TSut> where TSut : class
{
    private readonly Dictionary<Type, Mock> _mocks = [];

    /// <summary>
    /// Gets or creates a <see cref="Mock{TService}"/> instance for the specified dependency type.
    /// </summary>
    /// <typeparam name="TService">The service contract type to mock.</typeparam>
    /// <returns>The cached or newly created mock instance.</returns>
    protected Mock<TService> GetMock<TService>() where TService : class
    {
        var type = typeof(TService);
        if (!_mocks.TryGetValue(type, out var mock))
        {
            mock = new Mock<TService>();
            _mocks[type] = mock;
        }

        return (Mock<TService>)mock;
    }

    /// <summary>
    /// Resolves an instance of <typeparamref name="TService"/> using the managed mock object.
    /// </summary>
    /// <typeparam name="TService">The service contract type.</typeparam>
    /// <returns>The mock's object instance.</returns>
    protected TService GetMockObject<TService>() where TService : class => GetMock<TService>().Object;

    /// <summary>
    /// Verifies that all registered mocks have met their expected setups.
    /// </summary>
    protected void VerifyAllMocks()
    {
        foreach (var mock in _mocks.Values)
        {
            mock.VerifyAll();
        }
    }
}
