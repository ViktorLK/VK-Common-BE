using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.AI.Agents.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.UnitTests.Agents.Internal;

public class BasicAgentTests
{
    private readonly Mock<IVKChatEngine> _chatEngineMock;
    private readonly Mock<IOptions<VKAgentsOptions>> _optionsMock;
    private readonly Mock<IOptions<VKAIDefaultsOptions>> _globalOptionsMock;
    private readonly Mock<IVKUserContext> _userContextMock;
    private readonly Mock<ILogger<BasicAgent>> _loggerMock;

    public BasicAgentTests()
    {
        _chatEngineMock = new Mock<IVKChatEngine>();
        _optionsMock = new Mock<IOptions<VKAgentsOptions>>();
        _globalOptionsMock = new Mock<IOptions<VKAIDefaultsOptions>>();
        _userContextMock = new Mock<IVKUserContext>();
        _loggerMock = new Mock<ILogger<BasicAgent>>();

        _optionsMock.Setup(o => o.Value).Returns(new VKAgentsOptions { MaxIterations = 5 });
        _globalOptionsMock.Setup(o => o.Value).Returns(new VKAIDefaultsOptions { Timeout = TimeSpan.FromSeconds(30) });
        _userContextMock.Setup(u => u.TenantId).Returns("test-tenant");
    }

    private BasicAgent CreateAgent()
    {
        return new BasicAgent(
            "TestAgent",
            "A test agent",
            new List<IVKAtomicTool>(),
            new Dictionary<string, object>(),
            _chatEngineMock.Object,
            _optionsMock.Object,
            _globalOptionsMock.Object,
            _userContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_ReturnsSuccessResult()
    {
        // Arrange
        var agent = CreateAgent();
        var input = "Hello, Agent!";
        
        var chatResponse = new VKChatResponse
        {
            Message = VKChatMessage.FromText(VKChatRole.Assistant, "Hello, User!"),
            Usage = new VKAITokenUsage { InputTokens = 5, OutputTokens = 5 }
        };
            
        _chatEngineMock
            .Setup(c => c.SendAsync(
                It.IsAny<IReadOnlyList<VKChatMessage>>(),
                It.IsAny<VKChatArgs>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(chatResponse));

        // Act
        var result = await agent.ExecuteAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello, User!");
        
        _chatEngineMock.Verify(c => c.SendAsync(
            It.Is<IReadOnlyList<VKChatMessage>>(history => 
                history.Count >= 1 && 
                history[0].Role == VKChatRole.User && 
                history[0].Content == input),
            It.IsAny<VKChatArgs>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
