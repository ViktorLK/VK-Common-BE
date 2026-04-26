namespace VK.Blocks.Core.UnitTests.Guards;

public class VKGuardTests
{
    [Fact]
    public void NotNull_WithNonNullValue_ShouldReturnValue()
    {
        // Arrange
        var value = "test";

        // Act
        var result = VKGuard.NotNull(value);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void NotNull_WithNullValue_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => VKGuard.NotNull<string>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NotNullOrWhiteSpace_WithValidString_ShouldReturnString()
    {
        // Arrange
        var value = "test";

        // Act
        var result = VKGuard.NotNullOrWhiteSpace(value);

        // Assert
        result.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void NotNullOrWhiteSpace_WithInvalidString_ShouldThrowException(string? value)
    {
        // Act
        Action act = () => VKGuard.NotNullOrWhiteSpace(value!);

        // Assert
        if (value == null)
            act.Should().Throw<ArgumentNullException>();
        else
            act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NotEmpty_WithPopulatedCollection_ShouldReturnCollection()
    {
        // Arrange
        var collection = new List<int> { 1, 2, 3 };

        // Act
        var result = VKGuard.NotEmpty(collection);

        // Assert
        result.Should().BeEquivalentTo(collection);
    }

    [Fact]
    public void NotEmpty_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => VKGuard.NotEmpty<List<int>>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NotEmpty_WithEmptyCollection_ShouldThrowArgumentException()
    {
        // Arrange
        var collection = new List<int>();

        // Act
        Action act = () => VKGuard.NotEmpty(collection);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Positive_WithPositiveValue_ShouldReturnValue()
    {
        VKGuard.Positive(1).Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Positive_WithNonPositiveValue_ShouldThrowArgumentOutOfRangeException(int value)
    {
        Action act = () => VKGuard.Positive(value);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void NonNegative_WithZeroOrPositive_ShouldReturnValue()
    {
        VKGuard.NonNegative(0).Should().Be(0);
        VKGuard.NonNegative(1).Should().Be(1);
    }

    [Fact]
    public void NonNegative_WithNegativeValue_ShouldThrowArgumentOutOfRangeException()
    {
        Action act = () => VKGuard.NonNegative(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void InRange_WithinRange_ShouldReturnValue()
    {
        VKGuard.InRange(5, 1, 10).Should().Be(5);
        VKGuard.InRange(1, 1, 10).Should().Be(1);
        VKGuard.InRange(10, 1, 10).Should().Be(10);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void InRange_OutsideRange_ShouldThrowArgumentOutOfRangeException(int value)
    {
        Action act = () => VKGuard.InRange(value, 1, 10);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void NotDefault_WithNonDefaultValue_ShouldReturnValue()
    {
        VKGuard.NotDefault(1).Should().Be(1);
    }

    [Fact]
    public void NotDefault_WithDefaultValue_ShouldThrowArgumentException()
    {
        Action act = () => VKGuard.NotDefault(0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NotEmptyGuid_WithValidGuid_ShouldReturnValue()
    {
        var guid = Guid.NewGuid();
        VKGuard.NotEmptyGuid(guid).Should().Be(guid);
    }

    [Fact]
    public void NotEmptyGuid_WithEmptyGuid_ShouldThrowArgumentException()
    {
        Action act = () => VKGuard.NotEmptyGuid(Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Against_WithFalseCondition_ShouldNotThrow()
    {
        Action act = () => VKGuard.Against(false, "Error");
        act.Should().NotThrow();
    }

    [Fact]
    public void EnumDefined_WithDefinedValue_ShouldReturnValue()
    {
        VKGuard.EnumDefined(DayOfWeek.Monday).Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void EnumDefined_WithNotDefinedValue_ShouldThrowArgumentException()
    {
        Action act = () => VKGuard.EnumDefined((DayOfWeek)99);
        act.Should().Throw<ArgumentException>().WithMessage("*not defined in enum 'DayOfWeek'*");
    }

    [Fact]
    public void NotEmpty_WithIEnumerable_ShouldWorkCorrectly()
    {
        // Arrange
        IEnumerable<int> GetEnumerable()
        {
            yield return 1;
        }

        // Act & Assert
        VKGuard.NotEmpty(GetEnumerable()).Should().NotBeEmpty();
    }

    [Fact]
    public void Against_WithTrueCondition_ShouldThrowArgumentException()
    {
        Action act = () => VKGuard.Against(true, "Custom Error");
        act.Should().Throw<ArgumentException>().WithMessage("Custom Error*");
    }
}
