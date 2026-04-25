using VK.Blocks.Core.Security.Internal;

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
        PropertySecurityCache<SecureModel>.HasSensitiveProperties.Should().BeTrue();
    }

    [Fact]
    public void HasSensitiveProperties_WhenNoAttributes_ReturnsFalse()
    {
        PropertySecurityCache<NonSecureModel>.HasSensitiveProperties.Should().BeFalse();
    }

    [Fact]
    public void GetLevel_ReturnsCorrectLevel()
    {
        PropertySecurityCache<SecureModel>.GetLevel(nameof(SecureModel.NormalProperty)).Should().Be(SecurityLevel.None);
        PropertySecurityCache<SecureModel>.GetLevel(nameof(SecureModel.SensitiveProperty)).Should().Be(SecurityLevel.Sensitive);
        PropertySecurityCache<SecureModel>.GetLevel(nameof(SecureModel.RedactedProperty)).Should().Be(SecurityLevel.Redacted);
        PropertySecurityCache<SecureModel>.GetLevel("NonExistent").Should().Be(SecurityLevel.None);
    }

    [Fact]
    public void SensitivePropertyNames_ContainsExpectedNames()
    {
        var names = PropertySecurityCache<SecureModel>.SensitivePropertyNames.ToList();

        names.Should().Contain(nameof(SecureModel.SensitiveProperty));
        names.Should().Contain(nameof(SecureModel.RedactedProperty));
        names.Should().NotContain(nameof(SecureModel.NormalProperty));
    }
}
