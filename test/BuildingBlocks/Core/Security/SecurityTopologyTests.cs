namespace VK.Blocks.Core.UnitTests.Security;

public class SecurityTopologyTests
{
    [Fact]
    public void SecurityTopology_Initialization_ShouldSetProperties()
    {
        // Arrange & Act
        var topology = new VKSecurityTopology
        {
            Module = "Auth",
            Endpoints = [new VKSecurityMetadataEntry("Login", new { }, "Authentication")],
            Catalogs = new Dictionary<string, object> { ["Roles"] = new[] { "Admin" } }
        };

        // Assert
        topology.Module.Should().Be("Auth");
        topology.Endpoints.Should().HaveCount(1);
        topology.Catalogs.Should().ContainKey("Roles");
    }
}
