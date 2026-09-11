using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

/// <summary>
/// Unit tests for <see cref="VKKnowledgeOptions"/> validation rules.
/// Follows AP.01, DL.01, and BB.05.
/// </summary>
public sealed class KnowledgeOptionsValidatorTests : VKUnitTestBase
{
    private static IValidateOptions<VKKnowledgeOptions> CreateValidator()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddVKBlockMarker<VKCoreBlock>();
        services.AddVKBlockMarker<VKAIBlock>();

        var configuration = new ConfigurationBuilder().Build();
        services.AddVKAIPsycheBlock(configuration)
            .AddVKKnowledge();

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IValidateOptions<VKKnowledgeOptions>>();
    }

    [Fact]
    public void Validate_WhenOptionsAreDefaultNull_ShouldSucceed()
    {
        // Arrange
        var validator = CreateValidator();
        var options = new VKKnowledgeOptions
        {
            MaxEntriesToInject = null,
            ReservedTokens = null
        };

        // Act
        var result = validator.Validate(Options.DefaultName, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenOptionsArePositive_ShouldSucceed()
    {
        // Arrange
        var validator = CreateValidator();
        var options = new VKKnowledgeOptions
        {
            MaxEntriesToInject = 5,
            ReservedTokens = 1024
        };

        // Act
        var result = validator.Validate(Options.DefaultName, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Validate_WhenMaxEntriesToInjectNonPositive_ShouldFail(int invalidValue)
    {
        // Arrange
        var validator = CreateValidator();
        var options = new VKKnowledgeOptions
        {
            MaxEntriesToInject = invalidValue
        };

        // Act
        var result = validator.Validate(Options.DefaultName, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("MaxEntriesToInject, if set, must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Validate_WhenReservedTokensNonPositive_ShouldFail(int invalidValue)
    {
        // Arrange
        var validator = CreateValidator();
        var options = new VKKnowledgeOptions
        {
            ReservedTokens = invalidValue
        };

        // Act
        var result = validator.Validate(Options.DefaultName, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ReservedTokens, if set, must be greater than zero.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenDefaultXmlTagNullOrWhitespace_ShouldFail(string? invalidTag)
    {
        // Arrange
        var validator = CreateValidator();
        var options = new VKKnowledgeOptions
        {
            DefaultXmlTag = invalidTag!
        };

        // Act
        var result = validator.Validate(Options.DefaultName, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("DefaultXmlTag must not be null or whitespace.");
    }
}

