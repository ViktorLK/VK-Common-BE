namespace VK.Blocks.Core.UnitTests.Domain;

public class TestValueObject : VKValueObject
{
    public string Street { get; }
    public string City { get; }

    public TestValueObject(string street, string city)
    {
        Street = street;
        City = city;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
    }
}
