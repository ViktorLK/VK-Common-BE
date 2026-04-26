namespace VK.Blocks.Core.UnitTests.Domain;

public class ValueObjectTests
{
    [Fact]
    public void ValueObject_Equality_SameComponents_ReturnsTrue()
    {
        // Arrange
        var vo1 = new TestValueObject("123 Main St", "CityVille");
        var vo2 = new TestValueObject("123 Main St", "CityVille");

        // Act & Assert
        vo1.Equals(vo2).Should().BeTrue();
        (vo1 == vo2).Should().BeTrue();
        vo1.GetHashCode().Should().Be(vo2.GetHashCode());
    }

    [Fact]
    public void ValueObject_Equality_DifferentComponents_ReturnsFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("123 Main St", "CityVille");
        var vo2 = new TestValueObject("456 Elm St", "CityVille");

        // Act & Assert
        vo1.Equals(vo2).Should().BeFalse();
        (vo1 != vo2).Should().BeTrue();
        vo1.GetHashCode().Should().NotBe(vo2.GetHashCode());
    }

    [Fact]
    public void ValueObject_Equality_Null_ReturnsFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("123 Main St", "CityVille");
        TestValueObject? vo2 = null;

        // Act & Assert
        vo1.Equals(vo2).Should().BeFalse();
        (vo1 == vo2).Should().BeFalse();
        (vo1 != vo2).Should().BeTrue();
    }

    [Fact]
    public void ValueObject_Equality_DifferentValueObjectType_ReturnsFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("123 Main St", "CityVille");
        var vo2 = new OtherValueObject("123 Main St");

        // Act & Assert
        vo1.Equals(vo2).Should().BeFalse();
    }

    private sealed class OtherValueObject(string name) : VKValueObject
    {
        public string Name { get; } = name;
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Name;
        }
    }
}
