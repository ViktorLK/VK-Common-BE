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
        VKChatRole role = VKChatRole.System,
        string? name = null,
        string? tagName = null,
        int? absoluteDepth = null,
        VKPromptRelativeDepth? relativeDepth = null,
        int depthPriority = 0,
        int tokenCount = 0)
    {
        VKGuard.NotNull(content);

        return new VKPromptSegment
        {
            Content = content,
            Role = role,
            Name = name,
            TagName = tagName,
            AbsoluteDepth = absoluteDepth,
            RelativeDepth = relativeDepth,
            DepthPriority = Math.Clamp(depthPriority, 0, 999),
            TokenCount = tokenCount
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
        IReadOnlyDictionary<string, object>? extensions = null,
        int priority = 0,
        int tokenCount = 0)
    {
        return CreatePersona(new VKPersonaId(_guidGenerator.Create()), name, description, traits, extensions, priority, tokenCount);
    }

    /// <inheritdoc />
    public VKPersonaAnchor CreatePersona(
        VKPersonaId id,
        string name,
        string description,
        IReadOnlyDictionary<string, string>? traits = null,
        IReadOnlyDictionary<string, object>? extensions = null,
        int priority = 0,
        int tokenCount = 0)
    {
        return VKGuard.NotNull(VKPersonaAnchor.Create(id, name, description, traits, extensions, priority, tokenCount).Value);
    }

    // --- Directive ---

    /// <inheritdoc />
    public VKDirectiveCharter CreateDirective(
        string? overview = null,
        string? behaviorRules = null,
        string? safetyRules = null,
        string? outputConstraints = null,
        int priority = 0,
        int tokenCount = 0,
        string? name = null)
    {
        return CreateDirective(new VKDirectiveId(_guidGenerator.Create()), overview, behaviorRules, safetyRules, outputConstraints, priority, tokenCount, name);
    }

    /// <inheritdoc />
    public VKDirectiveCharter CreateDirective(
        VKDirectiveId id,
        string? overview = null,
        string? behaviorRules = null,
        string? safetyRules = null,
        string? outputConstraints = null,
        int priority = 0,
        int tokenCount = 0,
        string? name = null)
    {
        return VKGuard.NotNull(VKDirectiveCharter.Create(id, overview, behaviorRules, safetyRules, outputConstraints, priority, tokenCount, name).Value);
    }

    // --- Knowledge ---

    /// <inheritdoc />
    public VKKnowledgeEntry CreateKnowledge(
        VKPromptSegment segment,
        VKKnowledgeTriggerType triggerType = VKKnowledgeTriggerType.Constant,
        VKKnowledgeFilterLogic filterLogic = VKKnowledgeFilterLogic.AndAny,
        IReadOnlyList<VKKnowledgeKey>? keys = null,
        string? name = null)
    {
        return CreateKnowledge(new VKKnowledgeId(_guidGenerator.Create()), segment, triggerType, filterLogic, keys, name);
    }

    /// <inheritdoc />
    public VKKnowledgeEntry CreateKnowledge(
        VKKnowledgeId id,
        VKPromptSegment segment,
        VKKnowledgeTriggerType triggerType = VKKnowledgeTriggerType.Constant,
        VKKnowledgeFilterLogic filterLogic = VKKnowledgeFilterLogic.AndAny,
        IReadOnlyList<VKKnowledgeKey>? keys = null,
        string? name = null)
    {
        return VKGuard.NotNull(VKKnowledgeEntry.Create(id, segment, triggerType, filterLogic, keys, name).Value);
    }

    // --- Pattern ---

    /// <inheritdoc />
    public VKPatternEntry CreatePattern(VKPromptSegment segment, string? name = null)
    {
        return CreatePattern(new VKPatternId(_guidGenerator.Create()), segment, name);
    }

    /// <inheritdoc />
    public VKPatternEntry CreatePattern(VKPatternId id, VKPromptSegment segment, string? name = null)
    {
        return VKGuard.NotNull(VKPatternEntry.Create(id, segment, name).Value);
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
        string? forkPointRef = null)
    {
        var now = _timeProvider.GetUtcNow();
        return VKGuard.NotNull(VKSessionThread.Create(
            id: id,
            now: now,
            mode: mode,
            parentSessionId: parentSessionId,
            forkSourceSessionId: forkSourceSessionId,
            forkPointRef: forkPointRef).Value);
    }

    // --- Profile ---

    /// <inheritdoc />
    public VKProfilePresence CreateProfile(
        string? displayName = null,
        string? preferredLanguage = null,
        string? timeZone = null,
        string? description = null,
        VKPromptRelativeDepth? relativeDepth = VKPromptRelativeDepth.AfterDirective,
        int depthPriority = 10,
        int? absoluteDepth = null,
        string? tagName = null,
        int tokenCount = 0)
    {
        return CreateProfile(new VKProfileId(_guidGenerator.Create()), displayName, preferredLanguage, timeZone, description, relativeDepth, depthPriority, absoluteDepth, tagName, tokenCount);
    }

    /// <inheritdoc />
    public VKProfilePresence CreateProfile(
        VKProfileId id,
        string? displayName = null,
        string? preferredLanguage = null,
        string? timeZone = null,
        string? description = null,
        VKPromptRelativeDepth? relativeDepth = VKPromptRelativeDepth.AfterDirective,
        int depthPriority = 10,
        int? absoluteDepth = null,
        string? tagName = null,
        int tokenCount = 0)
    {
        return VKGuard.NotNull(VKProfilePresence.Create(id, displayName, preferredLanguage, timeZone, description, relativeDepth, depthPriority, absoluteDepth, tagName, tokenCount).Value);
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
