using VK.Blocks.Core;

namespace VK.Blocks.Core.UnitTests.Security.Internal;

public class PropertySecurityCacheTests
{
    private sealed class SecureModel
    {
        public string NormalProperty { get; set; } = string.Empty;

        [VKSensitiveData]
        public string SensitiveProperty { get; set; } = string.Empty;

        [VKRedacted]
        public string RedactedProperty { get; set; } = string.Empty;
    }

    private sealed class NonSecureModel
    {
        public int Id { get; set; }
    }

    [Fact]
    public void HasSensitiveProperties_WhenPropertiesHaveAttributes_ReturnsTrue()
    {
        VKPropertySecurityCache.HasSensitiveProperties<SecureModel>().Should().BeTrue();
    }

    [Fact]
    public void HasSensitiveProperties_WhenNoAttributes_ReturnsFalse()
    {
        VKPropertySecurityCache.HasSensitiveProperties<NonSecureModel>().Should().BeFalse();
    }

    [Fact]
    public void GetLevel_ReturnsCorrectLevel()
    {
        VKPropertySecurityCache.GetLevel<SecureModel>(nameof(SecureModel.NormalProperty)).Should().Be(VKSecurityLevel.None);
        VKPropertySecurityCache.GetLevel<SecureModel>(nameof(SecureModel.SensitiveProperty)).Should().Be(VKSecurityLevel.Sensitive);
        VKPropertySecurityCache.GetLevel<SecureModel>(nameof(SecureModel.RedactedProperty)).Should().Be(VKSecurityLevel.Redacted);
        VKPropertySecurityCache.GetLevel<SecureModel>("NonExistent").Should().Be(VKSecurityLevel.None);
    }

    [Fact]
    public void SensitivePropertyNames_ContainsExpectedNames()
    {
        var names = VKPropertySecurityCache.GetSensitivePropertyNames<SecureModel>().ToList();

        names.Should().Contain(nameof(SecureModel.SensitiveProperty));
        names.Should().Contain(nameof(SecureModel.RedactedProperty));
        names.Should().NotContain(nameof(SecureModel.NormalProperty));
    }

    [Fact]
    public void NonGenericMethods_ReturnSameResultsAsGeneric()
    {
        // HasSensitiveProperties(Type)
        VKPropertySecurityCache.HasSensitiveProperties(typeof(SecureModel)).Should().BeTrue();
        VKPropertySecurityCache.HasSensitiveProperties(typeof(NonSecureModel)).Should().BeFalse();

        // GetLevel(Type, string)
        VKPropertySecurityCache.GetLevel(typeof(SecureModel), nameof(SecureModel.SensitiveProperty)).Should().Be(VKSecurityLevel.Sensitive);
        VKPropertySecurityCache.GetLevel(typeof(SecureModel), nameof(SecureModel.RedactedProperty)).Should().Be(VKSecurityLevel.Redacted);
        VKPropertySecurityCache.GetLevel(typeof(SecureModel), nameof(SecureModel.NormalProperty)).Should().Be(VKSecurityLevel.None);
    }
}
