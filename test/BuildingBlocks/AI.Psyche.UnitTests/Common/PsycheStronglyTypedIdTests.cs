using System.ComponentModel;
using System.Text.Json;
using Moq;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Common;

// [AP.01] Sealed default
public sealed class PsycheStronglyTypedIdTests : VKUnitTestBase
{


    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void VKSessionId_FullContract_ShouldBehaveCorrectly()
    {
        var guid = Guid.NewGuid();
        GetMock<IVKGuidGenerator>().Setup(g => g.Create()).Returns(guid);
        var id = VKSessionId.New(GetMockObject<IVKGuidGenerator>());
        id.Value.Should().Be(guid);
        id.IsEmpty.Should().BeFalse();
        id.ToString().Should().Be(guid.ToString());

        // Empty & IsNullOrEmpty
        var empty = VKSessionId.Empty;
        empty.IsEmpty.Should().BeTrue();
        VKSessionId.IsNullOrEmpty(empty).Should().BeTrue();
        VKSessionId.IsNullOrEmpty(id).Should().BeFalse();
        VKSessionId? nullId = null;
        nullId.IsNullOrEmpty().Should().BeTrue();
        ((VKSessionId?)id).IsNullOrEmpty().Should().BeFalse();

        // Parse & TryParse
        VKSessionId.Parse(guid.ToString()).Should().Be(id);
        VKSessionId.TryParse(guid.ToString(), null, out var parsed).Should().BeTrue();
        parsed.Should().Be(id);
        VKSessionId.TryParse("invalid-guid", null, out _).Should().BeFalse();

        // Implicit operator
        VKSessionId implicitId = guid;
        implicitId.Should().Be(id);

        // CompareTo
        id.CompareTo(id).Should().Be(0);

        // JsonConverter
        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<VKSessionId>(json);
        deserialized.Should().Be(id);

        // TypeConverter
        var converter = TypeDescriptor.GetConverter(typeof(VKSessionId));
        converter.CanConvertFrom(typeof(string)).Should().BeTrue();
        var fromString = converter.ConvertFromString(guid.ToString());
        fromString.Should().Be(id);
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void VKDirectiveId_FullContract_ShouldBehaveCorrectly()
    {
        var guid = Guid.NewGuid();
        GetMock<IVKGuidGenerator>().Setup(g => g.Create()).Returns(guid);
        var id = VKDirectiveId.New(GetMockObject<IVKGuidGenerator>());
        id.Value.Should().Be(guid);
        id.IsEmpty.Should().BeFalse();

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<VKDirectiveId>(json);
        deserialized.Should().Be(id);

        var converter = TypeDescriptor.GetConverter(typeof(VKDirectiveId));
        var fromString = converter.ConvertFromString(guid.ToString());
        fromString.Should().Be(id);
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void VKEchoId_FullContract_ShouldBehaveCorrectly()
    {
        var guid = Guid.NewGuid();
        GetMock<IVKGuidGenerator>().Setup(g => g.Create()).Returns(guid);
        var id = VKEchoId.New(GetMockObject<IVKGuidGenerator>());
        id.Value.Should().Be(guid);
        id.IsEmpty.Should().BeFalse();

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<VKEchoId>(json);
        deserialized.Should().Be(id);

        var converter = TypeDescriptor.GetConverter(typeof(VKEchoId));
        var fromString = converter.ConvertFromString(guid.ToString());
        fromString.Should().Be(id);
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void VKKnowledgeId_FullContract_ShouldBehaveCorrectly()
    {
        var guid = Guid.NewGuid();
        GetMock<IVKGuidGenerator>().Setup(g => g.Create()).Returns(guid);
        var id = VKKnowledgeId.New(GetMockObject<IVKGuidGenerator>());
        id.Value.Should().Be(guid);
        id.IsEmpty.Should().BeFalse();

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<VKKnowledgeId>(json);
        deserialized.Should().Be(id);

        var converter = TypeDescriptor.GetConverter(typeof(VKKnowledgeId));
        var fromString = converter.ConvertFromString(guid.ToString());
        fromString.Should().Be(id);
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void VKPatternId_FullContract_ShouldBehaveCorrectly()
    {
        var guid = Guid.NewGuid();
        GetMock<IVKGuidGenerator>().Setup(g => g.Create()).Returns(guid);
        var id = VKPatternId.New(GetMockObject<IVKGuidGenerator>());
        id.Value.Should().Be(guid);
        id.IsEmpty.Should().BeFalse();

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<VKPatternId>(json);
        deserialized.Should().Be(id);

        var converter = TypeDescriptor.GetConverter(typeof(VKPatternId));
        var fromString = converter.ConvertFromString(guid.ToString());
        fromString.Should().Be(id);
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void VKPersonaId_FullContract_ShouldBehaveCorrectly()
    {
        var guid = Guid.NewGuid();
        GetMock<IVKGuidGenerator>().Setup(g => g.Create()).Returns(guid);
        var id = VKPersonaId.New(GetMockObject<IVKGuidGenerator>());
        id.Value.Should().Be(guid);
        id.IsEmpty.Should().BeFalse();

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<VKPersonaId>(json);
        deserialized.Should().Be(id);

        var converter = TypeDescriptor.GetConverter(typeof(VKPersonaId));
        var fromString = converter.ConvertFromString(guid.ToString());
        fromString.Should().Be(id);
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void VKProfileId_FullContract_ShouldBehaveCorrectly()
    {
        var guid = Guid.NewGuid();
        GetMock<IVKGuidGenerator>().Setup(g => g.Create()).Returns(guid);
        var id = VKProfileId.New(GetMockObject<IVKGuidGenerator>());
        id.Value.Should().Be(guid);
        id.IsEmpty.Should().BeFalse();

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<VKProfileId>(json);
        deserialized.Should().Be(id);

        var converter = TypeDescriptor.GetConverter(typeof(VKProfileId));
        var fromString = converter.ConvertFromString(guid.ToString());
        fromString.Should().Be(id);
    }
}
