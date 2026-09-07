using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI;
using VK.Blocks.AI.Synapse;
using VK.Blocks.AI.Synapse.Routing.Internal;
using VK.Blocks.AI.Synapse.UnitTests.Builders;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Routing;

/// <summary>
/// Unit tests for <see cref="DefaultAIProviderPool"/>.
/// Follows AP.01, CS.01, CS.03, and DL.01.
/// </summary>
public sealed class DefaultAIProviderPoolTests
{
    private readonly Mock<IVKIdentityContext> _identityContextMock = new();
    private readonly Mock<IVKAISynapseModelFactory> _modelFactoryMock = new();
    private readonly Mock<IVKAIConnectionStore> _connectionStoreMock = new();
    private readonly VKTenantId _tenantId = new(Guid.NewGuid());

    public DefaultAIProviderPoolTests()
    {
        _identityContextMock.Setup(i => i.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task GetAvailablePoolAsync_CombinesStaticOptionsAndTenantConnections()
    {
        // Arrange
        var staticOptionMock = new Mock<IVKAIProviderOptions>();
        staticOptionMock.Setup(o => o.Provider).Returns(VKAIProviderType.OpenAI);
        staticOptionMock.Setup(o => o.ModelId).Returns(VKAIModelIds.OpenAI.Gpt4O);

        var staticConnection = new VKAIConnectionBuilder()
            .WithId("static-openai")
            .WithProvider(VKAIProviderType.OpenAI)
            .Build();

        _modelFactoryMock.Setup(m => m.CreateConnection(
            It.IsAny<string>(),
            It.IsAny<string>(),
            VKAIProviderType.OpenAI,
            VKAIModelIds.OpenAI.Gpt4O,
            It.IsAny<string>(),
            It.IsAny<string>(),
            false,
            10,
            null)).Returns(staticConnection);

        var tenantConnection = new VKAIConnectionBuilder()
            .WithId("tenant-conn")
            .WithTenantId(_tenantId)
            .Build();

        var otherTenantConnection = new VKAIConnectionBuilder()
            .WithId("other-conn")
            .WithTenantId(new VKTenantId(Guid.NewGuid()))
            .Build();

        _connectionStoreMock.Setup(s => s.GetConnectionListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IEnumerable<VKAIConnection>>([tenantConnection, otherTenantConnection]));

        var pool = new DefaultAIProviderPool(
            _identityContextMock.Object,
            _modelFactoryMock.Object,
            [staticOptionMock.Object],
            _connectionStoreMock.Object);

        // Act
        var result = await pool.GetAvailablePoolAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(staticConnection);
        result.Value.Should().Contain(tenantConnection);
        result.Value.Should().NotContain(otherTenantConnection);
    }
}
