using VK.Blocks.AI.Psyche.Common.Internal;

namespace VK.Blocks.AI.Psyche.UnitTests.Common;

public sealed class DefaultPsycheModelFactoryTests : VKUnitTestBase
{
    [Fact]
    public void CreatePersona_CreatesValidPersonaAnchor()
    {
        // Arrange
        var newGuid = Guid.NewGuid();
        var fakeGuidGen = new VKFakeGuidGenerator(newGuid);
        var factory = new DefaultPsycheModelFactory(fakeGuidGen, TimeProvider.System);

        // Act
        var persona = factory.CreatePersona("Assistant", "Helper");

        // Assert
        persona.Id.Value.Should().Be(newGuid);
        persona.Name.Should().Be("Assistant");
        persona.Description.Should().Be("Helper");
    }

    [Fact]
    public void CreatePersona_WithFullParameters_CreatesValidPersonaAnchor()
    {
        // Arrange
        var newGuid = Guid.NewGuid();
        var fakeGuidGen = new VKFakeGuidGenerator(newGuid);
        var factory = new DefaultPsycheModelFactory(fakeGuidGen, TimeProvider.System);
        var traits = new Dictionary<string, string> { ["Tone"] = "Warm" };
        var extensions = new Dictionary<string, object> { ["V"] = 2 };

        // Act
        var persona = factory.CreatePersona("Assistant", "Helper", traits, extensions);

        // Assert
        persona.Id.Value.Should().Be(newGuid);
        persona.Traits.Should().ContainKey("Tone");
        persona.Extensions.Should().ContainKey("V");
    }

    [Fact]
    public void CreateDirective_CreatesValidDirectiveCharter()
    {
        // Arrange
        var newGuid = Guid.NewGuid();
        var fakeGuidGen = new VKFakeGuidGenerator(newGuid);
        var factory = new DefaultPsycheModelFactory(fakeGuidGen, TimeProvider.System);

        // Act
        var directive = factory.CreateDirective("Overview", "Rules", "Safety", "Constraints");

        // Assert
        directive.Id.Value.Should().Be(newGuid);
        directive.Overview.Should().Be("Overview");
        directive.BehaviorRules.Should().Be("Rules");
        directive.SafetyRules.Should().Be("Safety");
        directive.OutputConstraints.Should().Be("Constraints");
    }

    [Fact]
    public void CreateKnowledge_CreatesValidKnowledgeEntry()
    {
        // Arrange
        var newGuid = Guid.NewGuid();
        var fakeGuidGen = new VKFakeGuidGenerator(newGuid);
        var factory = new DefaultPsycheModelFactory(fakeGuidGen, TimeProvider.System);
        var segment = new VKPromptSegment { Content = "Knowledge Item" };

        // Act
        var entry = factory.CreateKnowledge(
            segment,
            VKKnowledgeTriggerType.Keyword,
            VKKnowledgeFilterLogic.AndAll,
            xmlTag: "lore",
            keys: [new VKKnowledgeKey { Text = "dragon" }]);

        // Assert
        entry.Id.Value.Should().Be(newGuid);
        entry.Segment.Content.Should().Be("Knowledge Item");
        entry.TriggerType.Should().Be(VKKnowledgeTriggerType.Keyword);
        entry.FilterLogic.Should().Be(VKKnowledgeFilterLogic.AndAll);
        entry.XmlTag.Should().Be("lore");
        entry.Keys.Should().HaveCount(1);
    }

    [Fact]
    public void CreatePattern_CreatesValidPatternEntry()
    {
        // Arrange
        var newGuid = Guid.NewGuid();
        var fakeGuidGen = new VKFakeGuidGenerator(newGuid);
        var factory = new DefaultPsycheModelFactory(fakeGuidGen, TimeProvider.System);
        var segment = new VKPromptSegment { Content = "Pattern Item" };

        // Act
        var entry = factory.CreatePattern(segment);

        // Assert
        entry.Id.Value.Should().Be(newGuid);
        entry.Segment.Content.Should().Be("Pattern Item");
    }

    [Fact]
    public void CreateSession_CreatesValidSessionThread()
    {
        // Arrange
        var newGuid = Guid.NewGuid();
        var fakeGuidGen = new VKFakeGuidGenerator(newGuid);
        var factory = new DefaultPsycheModelFactory(fakeGuidGen, TimeProvider.System);

        // Act
        var session = factory.CreateSession(VKSessionMode.Isolated);

        // Assert
        session.Id.Value.Should().Be(newGuid);
        session.Mode.Should().Be(VKSessionMode.Isolated);
        session.Status.Should().Be(VKSessionStatus.Active);
    }

    [Fact]
    public void CreateSession_WithFullParameters_CreatesRehydratedSession()
    {
        // Arrange
        var fakeGuidGen = new VKFakeGuidGenerator();
        var factory = new DefaultPsycheModelFactory(fakeGuidGen, TimeProvider.System);
        var sessionId = new VKSessionId(Guid.NewGuid());
        var parentSessionId = new VKSessionId(Guid.NewGuid());
        var forkSourceSessionId = new VKSessionId(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;
        var kState = new VKSessionKnowledgeState { LastEvaluatedTurn = 5 };

        // Act
        var session = factory.CreateSession(
            sessionId,
            VKSessionMode.Continuous,
            parentSessionId,
            forkSourceSessionId,
            forkPointRef: "cp-1",
            status: VKSessionStatus.Archived,
            turnCount: 5,
            createdAt: now.AddHours(-1),
            updatedAt: now,
            lastActivityAt: now,
            knowledgeState: kState);

        // Assert
        session.Id.Should().Be(sessionId);
        session.Mode.Should().Be(VKSessionMode.Continuous);
        session.ParentSessionId.Should().Be(parentSessionId);
        session.ForkSourceSessionId.Should().Be(forkSourceSessionId);
        session.ForkPointRef.Should().Be("cp-1");
        session.Status.Should().Be(VKSessionStatus.Archived);
        session.TurnCount.Should().Be(5);
        session.KnowledgeState.Should().BeSameAs(kState);
    }

    [Fact]
    public void CreateProfile_CreatesValidProfilePresence()
    {
        // Arrange
        var newGuid = Guid.NewGuid();
        var fakeGuidGen = new VKFakeGuidGenerator(newGuid);
        var factory = new DefaultPsycheModelFactory(fakeGuidGen, TimeProvider.System);
        var prefs = new Dictionary<string, string> { ["Theme"] = "Dark" };

        // Act
        var profile = factory.CreateProfile("Hero", "zh-CN", "UTC", prefs);

        // Assert
        profile.Id.Value.Should().Be(newGuid);
        profile.DisplayName.Should().Be("Hero");
        profile.PreferredLanguage.Should().Be("zh-CN");
        profile.TimeZone.Should().Be("UTC");
        profile.Preferences.Should().ContainKey("Theme");
    }

    [Fact]
    public void CreateEcho_CreatesValidEchoTrace()
    {
        // Arrange
        var newGuid = Guid.NewGuid();
        var fakeGuidGen = new VKFakeGuidGenerator(newGuid);
        var factory = new DefaultPsycheModelFactory(fakeGuidGen, TimeProvider.System);
        var sessionId = new VKSessionId(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;

        // Act
        var trace = factory.CreateEcho(sessionId, VKChatRole.User, "User input message", tokenCount: 42, createdAt: now);

        // Assert
        trace.Id.Value.Should().Be(newGuid);
        trace.SessionId.Should().Be(sessionId);
        trace.Content.Should().Be("User input message");
        trace.Role.Should().Be(VKChatRole.User);
        trace.TokenCount.Should().Be(42);
        trace.CreatedAt.Should().Be(now);
    }

    [Fact]
    public void CreateSegmentAndKey_ReturnsExpectedInstances()
    {
        // Arrange
        var fakeGuidGen = new VKFakeGuidGenerator();
        var factory = new DefaultPsycheModelFactory(fakeGuidGen, TimeProvider.System);

        // Act
        var segment = factory.CreateSegment(
            "Content",
            name: "Seg1",
            isEnabled: true,
            role: VKChatRole.System,
            absoluteDepth: 1,
            relativeDepth: VKPromptRelativeDepth.AfterEcho,
            depthPriority: 100);
        var key = factory.CreateKey("term", VKKnowledgeMatchType.WholeWord, caseSensitive: true);

        // Assert
        segment.Content.Should().Be("Content");
        segment.Name.Should().Be("Seg1");
        segment.AbsoluteDepth.Should().Be(1);
        segment.RelativeDepth.Should().Be(VKPromptRelativeDepth.AfterEcho);
        segment.DepthPriority.Should().Be(100);

        key.Text.Should().Be("term");
        key.MatchType.Should().Be(VKKnowledgeMatchType.WholeWord);
        key.CaseSensitive.Should().BeTrue();
    }

    [Fact]
    public void ExplicitIdOverloads_CreateValidModels()
    {
        // Arrange
        var fakeGuidGen = new VKFakeGuidGenerator();
        var factory = new DefaultPsycheModelFactory(fakeGuidGen, TimeProvider.System);
        var personaId = new VKPersonaId(Guid.NewGuid());
        var directiveId = new VKDirectiveId(Guid.NewGuid());
        var knowledgeId = new VKKnowledgeId(Guid.NewGuid());
        var patternId = new VKPatternId(Guid.NewGuid());
        var sessionId = new VKSessionId(Guid.NewGuid());
        var profileId = new VKProfileId(Guid.NewGuid());
        var echoId = new VKEchoId(Guid.NewGuid());
        var segment = new VKPromptSegment { Content = "Segment" };

        // Act
        var persona = factory.CreatePersona(personaId, "P", "Desc");
        var directive = factory.CreateDirective(directiveId, "Over");
        var knowledge = factory.CreateKnowledge(knowledgeId, segment);
        var pattern = factory.CreatePattern(patternId, segment);
        var session = factory.CreateSession(sessionId, VKSessionMode.Continuous);
        var profile = factory.CreateProfile(profileId, "Prof");
        var echo = factory.CreateEcho(echoId, sessionId, VKChatRole.Assistant, "Reply");

        // Assert
        persona.Id.Should().Be(personaId);
        directive.Id.Should().Be(directiveId);
        knowledge.Id.Should().Be(knowledgeId);
        pattern.Id.Should().Be(patternId);
        session.Id.Should().Be(sessionId);
        profile.Id.Should().Be(profileId);
        echo.Id.Should().Be(echoId);
    }
}
