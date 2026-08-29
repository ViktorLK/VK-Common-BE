using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Common.Internal;

/// <summary>
/// Default implementation of <see cref="IVKPsycheModelFactory"/> which binds
/// <see cref="IVKGuidGenerator"/> (CS.06) and <see cref="TimeProvider"/> (CS.06) to Psyche models.
/// Follows AP.01.
/// </summary>
internal sealed class DefaultPsycheModelFactory(
    IVKGuidGenerator guidGenerator,
    TimeProvider timeProvider) : IVKPsycheModelFactory
{
    private readonly IVKGuidGenerator _guidGenerator = VKGuard.NotNull(guidGenerator);
    private readonly TimeProvider _timeProvider = VKGuard.NotNull(timeProvider);

    // --- Segment & Key ---

    /// <inheritdoc />
    public VKPromptSegment CreateSegment(
        string content,
        string? name = null,
        bool isEnabled = true,
        VKChatRole role = VKChatRole.System,
        int? absoluteDepth = null,
        VKPromptRelativeDepth? relativeDepth = null,
        int depthPriority = 0)
    {
        VKGuard.NotNull(content);

        return new VKPromptSegment
        {
            Content = content,
            Name = name,
            IsEnabled = isEnabled,
            Role = role,
            AbsoluteDepth = absoluteDepth,
            RelativeDepth = relativeDepth,
            DepthPriority = Math.Clamp(depthPriority, 0, 999)
        };
    }

    /// <inheritdoc />
    public VKKnowledgeKey CreateKey(
        string text,
        VKKnowledgeMatchType matchType = VKKnowledgeMatchType.Contains,
        bool caseSensitive = false)
    {
        VKGuard.NotNull(text);

        return new VKKnowledgeKey
        {
            Text = text,
            MatchType = matchType,
            CaseSensitive = caseSensitive
        };
    }

    // --- Persona ---

    /// <inheritdoc />
    public VKPersonaAnchor CreatePersona(
        string name,
        string description,
        IReadOnlyDictionary<string, string>? traits = null,
        IReadOnlyDictionary<string, object>? extensions = null)
    {
        return CreatePersona(new VKPersonaId(_guidGenerator.Create()), name, description, traits, extensions);
    }

    /// <inheritdoc />
    public VKPersonaAnchor CreatePersona(
        VKPersonaId id,
        string name,
        string description,
        IReadOnlyDictionary<string, string>? traits = null,
        IReadOnlyDictionary<string, object>? extensions = null)
    {
        return VKPersonaAnchor.Create(id, name, description, traits, extensions).Value;
    }

    // --- Directive ---

    /// <inheritdoc />
    public VKDirectiveCharter CreateDirective(
        string? overview = null,
        string? behaviorRules = null,
        string? safetyRules = null,
        string? outputConstraints = null)
    {
        return CreateDirective(new VKDirectiveId(_guidGenerator.Create()), overview, behaviorRules, safetyRules, outputConstraints);
    }

    /// <inheritdoc />
    public VKDirectiveCharter CreateDirective(
        VKDirectiveId id,
        string? overview = null,
        string? behaviorRules = null,
        string? safetyRules = null,
        string? outputConstraints = null)
    {
        return VKDirectiveCharter.Create(id, overview, behaviorRules, safetyRules, outputConstraints).Value;
    }

    // --- Knowledge ---

    /// <inheritdoc />
    public VKKnowledgeEntry CreateKnowledge(
        VKPromptSegment segment,
        VKKnowledgeTriggerType triggerType = VKKnowledgeTriggerType.Constant,
        VKKnowledgeFilterLogic filterLogic = VKKnowledgeFilterLogic.AndAny,
        string? xmlTag = null,
        IReadOnlyList<VKKnowledgeKey>? keys = null)
    {
        return CreateKnowledge(new VKKnowledgeId(_guidGenerator.Create()), segment, triggerType, filterLogic, xmlTag, keys);
    }

    /// <inheritdoc />
    public VKKnowledgeEntry CreateKnowledge(
        VKKnowledgeId id,
        VKPromptSegment segment,
        VKKnowledgeTriggerType triggerType = VKKnowledgeTriggerType.Constant,
        VKKnowledgeFilterLogic filterLogic = VKKnowledgeFilterLogic.AndAny,
        string? xmlTag = null,
        IReadOnlyList<VKKnowledgeKey>? keys = null)
    {
        return VKKnowledgeEntry.Create(id, segment, triggerType, filterLogic, xmlTag, keys).Value;
    }

    // --- Pattern ---

    /// <inheritdoc />
    public VKPatternEntry CreatePattern(VKPromptSegment segment)
    {
        return CreatePattern(new VKPatternId(_guidGenerator.Create()), segment);
    }

    /// <inheritdoc />
    public VKPatternEntry CreatePattern(VKPatternId id, VKPromptSegment segment)
    {
        return VKPatternEntry.Create(id, segment).Value;
    }

    // --- Session ---

    /// <inheritdoc />
    public VKSessionThread CreateSession(
        VKSessionMode mode = VKSessionMode.Isolated,
        VKSessionId? parentSessionId = null,
        VKSessionId? forkSourceSessionId = null,
        string? forkPointRef = null)
    {
        return CreateSession(new VKSessionId(_guidGenerator.Create()), mode, parentSessionId, forkSourceSessionId, forkPointRef);
    }

    /// <inheritdoc />
    public VKSessionThread CreateSession(
        VKSessionId id,
        VKSessionMode mode = VKSessionMode.Isolated,
        VKSessionId? parentSessionId = null,
        VKSessionId? forkSourceSessionId = null,
        string? forkPointRef = null,
        VKSessionStatus status = VKSessionStatus.Active,
        int turnCount = 0,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null,
        DateTimeOffset? lastActivityAt = null,
        VKSessionKnowledgeState? knowledgeState = null)
    {
        var now = _timeProvider.GetUtcNow();
        return VKSessionThread.Rehydrate(
            id: id,
            mode: mode,
            parentSessionId: parentSessionId,
            forkSourceSessionId: forkSourceSessionId,
            forkPointRef: forkPointRef,
            status: status,
            turnCount: turnCount,
            knowledgeState: knowledgeState ?? new VKSessionKnowledgeState(),
            createdAt: createdAt ?? now,
            updatedAt: updatedAt ?? now,
            lastActivityAt: lastActivityAt);
    }

    // --- Profile ---

    /// <inheritdoc />
    public VKProfilePresence CreateProfile(
        string? displayName = null,
        string? preferredLanguage = null,
        string? timeZone = null,
        IReadOnlyDictionary<string, string>? preferences = null)
    {
        return CreateProfile(new VKProfileId(_guidGenerator.Create()), displayName, preferredLanguage, timeZone, preferences);
    }

    /// <inheritdoc />
    public VKProfilePresence CreateProfile(
        VKProfileId id,
        string? displayName = null,
        string? preferredLanguage = null,
        string? timeZone = null,
        IReadOnlyDictionary<string, string>? preferences = null)
    {
        return VKProfilePresence.Create(id, displayName, preferredLanguage, timeZone, preferences).Value;
    }

    // --- Echo ---

    /// <inheritdoc />
    public VKEchoTrace CreateEcho(
        VKSessionId sessionId,
        VKChatRole role,
        string content,
        int tokenCount = 0,
        DateTimeOffset? createdAt = null)
    {
        return CreateEcho(new VKEchoId(_guidGenerator.Create()), sessionId, role, content, tokenCount, createdAt);
    }

    /// <inheritdoc />
    public VKEchoTrace CreateEcho(
        VKEchoId id,
        VKSessionId sessionId,
        VKChatRole role,
        string content,
        int tokenCount = 0,
        DateTimeOffset? createdAt = null)
    {
        VKGuard.NotNull(content);

        return new VKEchoTrace
        {
            SessionId = sessionId,
            Id = id,
            Role = role,
            Content = content,
            TokenCount = tokenCount,
            CreatedAt = createdAt ?? _timeProvider.GetUtcNow()
        };
    }
}
