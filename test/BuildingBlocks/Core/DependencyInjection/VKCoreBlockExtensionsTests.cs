using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core.Guids.Internal;
using VK.Blocks.Core.Serialization.Internal;

namespace VK.Blocks.Core.UnitTests.DependencyInjection;

public sealed class VKCoreBlockExtensionsTests
{
    private readonly ServiceCollection _services = new ServiceCollection();
    private readonly IConfiguration _configuration = new ConfigurationBuilder().Build();

    [Fact]
    public void AddVKCoreBlock_ShouldRegisterCoreServices()
    {
        // Act
        _services.AddVKCoreBlock(_configuration);

        // Assert
        var provider = _services.BuildServiceProvider();

        provider.GetService<TimeProvider>().Should().Be(TimeProvider.System);
        provider.GetService<IVKGuidGenerator>().Should().BeOfType<SequentialGuidGenerator>();
        provider.GetService<IVKJsonSerializer>().Should().BeOfType<SystemTextJsonSerializer>();

        // Marker check
        provider.GetService<VKCoreBlock>().Should().NotBeNull();
    }

    [Fact]
    public void AddVKCoreBlock_IsIdempotent()
    {
        // Act
        _services.AddVKCoreBlock(_configuration);
        var countBefore = _services.Count;

        _services.AddVKCoreBlock(_configuration); // Second call

        // Assert
        _services.Count.Should().Be(countBefore);
    }
}
