using FluentAssertions;
using VK.Blocks.Core.Guards;

namespace VK.Blocks.Core.UnitTests.Guards;

public class GuardTests
{
    [Fact]
    public void NotNull_ValidValue_ReturnsValue()
    {
        // Arrange
        var input = new object();

        // Act
        var result = Guard.NotNull(input, nameof(input));

        // Assert
        result.Should().BeSameAs(input);
    }

    [Fact]
    public void NotNull_NullValue_ThrowsArgumentNullException()
    {
        // Arrange
        object? input = null;

        // Act
        Action act = () => Guard.NotNull(input!, nameof(input));

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName(nameof(input));
    }

    [Theory]
    [InlineData("valid string")]
    [InlineData(" a ")]
    public void NotNullOrWhiteSpace_ValidString_ReturnsString(string input)
    {
        // Arrange & Act
        var result = Guard.NotNullOrWhiteSpace(input, nameof(input));

        // Assert
        result.Should().Be(input);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void NotNullOrWhiteSpace_InvalidString_ThrowsArgumentException(string? input)
    {
        // Arrange & Act
        Action act = () => Guard.NotNullOrWhiteSpace(input, nameof(input));

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Value cannot be null or whitespace.*")
           .And.ParamName.Should().Be(nameof(input));
    }

    [Fact]
    public void NotDefault_NonDefaultValue_ReturnsValue()
    {
        // Arrange
        var input = Guid.NewGuid();

        // Act
        var result = Guard.NotDefault(input, nameof(input));

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void NotDefault_DefaultValue_ThrowsArgumentException()
    {
        // Arrange
        var input = default(Guid);

        // Act
        Action act = () => Guard.NotDefault(input, nameof(input));

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Value cannot be the default value of Guid.*")
           .And.ParamName.Should().Be(nameof(input));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Positive_PositiveValue_ReturnsValue(int input)
    {
        // Arrange & Act
        var result = Guard.Positive(input, nameof(input));

        // Assert
        result.Should().Be(input);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Positive_NonPositiveValue_ThrowsArgumentOutOfRangeException(int input)
    {
        // Arrange & Act
        Action act = () => Guard.Positive(input, nameof(input));

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithMessage("*Value must be positive.*")
           .And.ParamName.Should().Be(nameof(input));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void NonNegative_NonNegativeValue_ReturnsValue(int input)
    {
        // Arrange & Act
        var result = Guard.NonNegative(input, nameof(input));

        // Assert
        result.Should().Be(input);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void NonNegative_NegativeValue_ThrowsArgumentOutOfRangeException(int input)
    {
        // Arrange & Act
        Action act = () => Guard.NonNegative(input, nameof(input));

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithMessage("*Value must be non-negative.*")
           .And.ParamName.Should().Be(nameof(input));
    }
}
