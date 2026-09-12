using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

public sealed class DefaultKnowledgeMatcherTests : VKUnitTestBase
{
    [Fact]
    public void GetMatcher_WithConstantTrigger_AlwaysReturnsTrue()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithTriggerType(VKKnowledgeTriggerType.Constant)
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("any context string").Should().BeTrue();
    }

    [Fact]
    public void GetMatcher_WithEmptyKeys_ReturnsFalse()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKeys([])
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("hello world").Should().BeFalse();
    }

    [Fact]
    public void GetMatcher_WithEmptyKeyText_ReturnsFalse()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = "   ", MatchType = VKKnowledgeMatchType.Contains })
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("any input").Should().BeFalse();
    }

    [Fact]
    public void GetMatcher_WithKeywordContains_MatchesCorrectly()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Refund Policy")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithFilterLogic(VKKnowledgeFilterLogic.AndAny)
            .WithKey(new VKKnowledgeKey { Text = "refund", MatchType = VKKnowledgeMatchType.Contains, CaseSensitive = false })
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("I want a refund for my order").Should().BeTrue();
        matcher("Help with shipping").Should().BeFalse();
    }

    [Fact]
    public void GetMatcher_WithCaseSensitive_MatchesExactCaseOnly()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = "Important", MatchType = VKKnowledgeMatchType.Contains, CaseSensitive = true })
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("This is Important.").Should().BeTrue();
        matcher("This is important.").Should().BeFalse();
    }

    [Fact]
    public void GetMatcher_WithRegexPattern_MatchesCorrectly()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Order Status")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = @"/order-\d+/i", MatchType = VKKnowledgeMatchType.Regex })
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("Status for ORDER-12345 please").Should().BeTrue();
        matcher("Status for order xyz").Should().BeFalse();
    }

    [Fact]
    public void GetMatcher_WithMalformedRegex_FallbackReturnsFalse()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Broken Rule")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = @"[unclosed-bracket", MatchType = VKKnowledgeMatchType.Regex })
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("test input").Should().BeFalse();
    }

    [Fact]
    public void Invalidate_ClearsCachedMatcher()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithTriggerType(VKKnowledgeTriggerType.Constant)
            .Build();

        var matcher1 = DefaultKnowledgeMatcher.GetMatcher(entry);
        DefaultKnowledgeMatcher.Invalidate(entry.Id);
        DefaultKnowledgeMatcher.Invalidate(VKKnowledgeId.Empty);

        // Act & Assert
        matcher1("test").Should().BeTrue();
    }

    [Fact]
    public void GetMatcher_WithWholeWord_MatchesWordBoundariesOnly()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Cat knowledge")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = "cat", MatchType = VKKnowledgeMatchType.WholeWord, CaseSensitive = false })
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("The cat is here").Should().BeTrue();
        matcher("scattered words").Should().BeFalse();
    }

    [Fact]
    public void GetMatcher_WithFilterLogics_EvaluatesCorrectly()
    {
        // Arrange
        var andAllEntry = new VKKnowledgeEntryBuilder()
            .WithContent("Both required")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithFilterLogic(VKKnowledgeFilterLogic.AndAll)
            .WithKey(new VKKnowledgeKey { Text = "alpha", MatchType = VKKnowledgeMatchType.Contains })
            .WithKey(new VKKnowledgeKey { Text = "beta", MatchType = VKKnowledgeMatchType.Contains })
            .Build();

        var notAnyEntry = new VKKnowledgeEntryBuilder()
            .WithContent("None forbidden")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithFilterLogic(VKKnowledgeFilterLogic.NotAny)
            .WithKey(new VKKnowledgeKey { Text = "forbidden", MatchType = VKKnowledgeMatchType.Contains })
            .Build();

        var notAllEntry = new VKKnowledgeEntryBuilder()
            .WithContent("Not both")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithFilterLogic(VKKnowledgeFilterLogic.NotAll)
            .WithKey(new VKKnowledgeKey { Text = "alpha", MatchType = VKKnowledgeMatchType.Contains })
            .WithKey(new VKKnowledgeKey { Text = "beta", MatchType = VKKnowledgeMatchType.Contains })
            .Build();

        // Act
        var andAllMatcher = DefaultKnowledgeMatcher.GetMatcher(andAllEntry);
        var notAnyMatcher = DefaultKnowledgeMatcher.GetMatcher(notAnyEntry);
        var notAllMatcher = DefaultKnowledgeMatcher.GetMatcher(notAllEntry);

        // Assert
        andAllMatcher("alpha only").Should().BeFalse();
        andAllMatcher("alpha and beta").Should().BeTrue();

        notAnyMatcher("clean sentence").Should().BeTrue();
        notAnyMatcher("forbidden sentence").Should().BeFalse();

        notAllMatcher("alpha and beta").Should().BeFalse();
        notAllMatcher("alpha only").Should().BeTrue();

        // Cache hit test
        DefaultKnowledgeMatcher.GetMatcher(andAllEntry).Should().BeSameAs(andAllMatcher);
    }

    [Fact]
    public void GetMatcher_WithRegexFlags_MatchesMultilineAndSingleline()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Multiline test")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = @"/^start.*end$/ims", MatchType = VKKnowledgeMatchType.Regex })
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("start\nmiddle\nend").Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetMatcher_WithNullOrWhitespaceInput_ReturnsFalseEvenForNegativeFilters(string? input)
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Empty check")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithFilterLogic(VKKnowledgeFilterLogic.NotAny)
            .WithKey(new VKKnowledgeKey { Text = "forbidden", MatchType = VKKnowledgeMatchType.Contains })
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert: Even with NotAny logic, empty/whitespace/null text must not activate knowledge
        matcher(input!).Should().BeFalse();
    }

    [Fact]
    public void GetMatcher_WithBareRegexPattern_MatchesCorrectly()
    {
        // Arrange (Regex without surrounding slashes)
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Bare Regex Rule")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = @"\bID-\d{4}\b", MatchType = VKKnowledgeMatchType.Regex, CaseSensitive = false })
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("User ID-5678 is active").Should().BeTrue();
        matcher("User id-5678 is active").Should().BeTrue();
        matcher("User ID-abcd is active").Should().BeFalse();
    }

    [Fact]
    public void GetMatcher_WhenEntryModified_InvalidatesCacheAndRecompiles()
    {
        // Arrange
        var initialEntry = new VKKnowledgeEntryBuilder()
            .WithContent("Version 1")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = "version1", MatchType = VKKnowledgeMatchType.Contains })
            .Build();

        var matcher1 = DefaultKnowledgeMatcher.GetMatcher(initialEntry);
        matcher1("this is version1").Should().BeTrue();
        matcher1("this is version2").Should().BeFalse();

        // Create updated entry with the EXACT same Id, but different keys
        var updatedEntry = new VKKnowledgeEntryBuilder()
            .WithId(initialEntry.Id)
            .WithContent("Version 2")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = "version2", MatchType = VKKnowledgeMatchType.Contains })
            .Build();

        // Act
        var matcher2 = DefaultKnowledgeMatcher.GetMatcher(updatedEntry);

        // Assert (Hash changed, so new matcher is returned)
        matcher2.Should().NotBeSameAs(matcher1);
        matcher2("this is version2").Should().BeTrue();
        matcher2("this is version1").Should().BeFalse();
    }

    [Fact]
    public void GetMatcher_WithMixedKeyTypesAndAll_RequiresBothMatches()
    {
        // Arrange (Mixed Contains + Regex under AndAll)
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Mixed Rules")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithFilterLogic(VKKnowledgeFilterLogic.AndAll)
            .WithKey(new VKKnowledgeKey { Text = "urgent", MatchType = VKKnowledgeMatchType.Contains, CaseSensitive = false })
            .WithKey(new VKKnowledgeKey { Text = @"#\d{4}", MatchType = VKKnowledgeMatchType.Regex })
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("This is an urgent ticket #1234").Should().BeTrue();
        matcher("This is urgent without ticket number").Should().BeFalse();
        matcher("Normal ticket #1234").Should().BeFalse();
    }

    [Fact]
    public void GetMatcher_WithNullEntry_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => DefaultKnowledgeMatcher.GetMatcher(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetMatcher_WithWholeWordAndCjkKeyword_FallsBackToSubstringMatch()
    {
        // Arrange (CJK characters without space delimiters in Chinese text)
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Apple knowledge")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = "苹果", MatchType = VKKnowledgeMatchType.WholeWord, CaseSensitive = false })
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert: Chinese text without ASCII word boundaries should match successfully
        matcher("我想吃苹果啊").Should().BeTrue();
        matcher("今天去买香蕉").Should().BeFalse();
    }

    [Theory]
    [InlineData("日本語", "私は日本語を勉強しています", true)]
    [InlineData("桜", "春には桜が満開になります", true)]
    [InlineData("佐々木", "佐々木さんはエンジニアです", true)]
    [InlineData("コーヒー", "毎朝コーヒーを飲みます", true)]
    [InlineData("富士山", "今日はいい天気です", false)]
    public void GetMatcher_WithWholeWordAndJapaneseKeywords_FallsBackToSubstringMatch(string keyword, string input, bool expected)
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Japanese test")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = keyword, MatchType = VKKnowledgeMatchType.WholeWord, CaseSensitive = false })
            .Build();

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher(input).Should().Be(expected);
    }
}
