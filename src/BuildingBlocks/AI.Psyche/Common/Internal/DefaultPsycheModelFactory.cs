using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Common.Internal;

/// <summary>
/// Default implementation of <see cref="IVKPsycheModelFactory"/> which binds current <see cref="IVKIdentityContext"/>,
/// <see cref="IVKGuidGenerator"/> (CS.06), and <see cref="TimeProvider"/> (CS.06) to Psyche models.
/// Follows AP.01.
/// </summary>
internal sealed class DefaultPsycheModelFactory(
    IVKIdentityContext identityContext,
    IVKGuidGenerator guidGenerator,
    TimeProvider timeProvider) : IVKPsycheModelFactory
{
    private readonly IVKIdentityContext _identityContext = VKGuard.NotNull(identityContext);
    private readonly IVKGuidGenerator _guidGenerator = VKGuard.NotNull(guidGenerator);
    private readonly TimeProvider _timeProvider = VKGuard.NotNull(timeProvider);

    // --- Persona ---

    /// <inheritdoc />
    public VKPersonaAnchor CreatePersona(
        string name,
        string description,
        IReadOnlyDictionary<string, string>? traits = null,
        string? directiveId = null,
        IReadOnlyDictionary<string, object>? extensions = null)
    {
        return CreatePersona(new VKPersonaId(_guidGenerator.Create()), name, description, traits, directiveId, extensions);
    }

    /// <inheritdoc />
    public VKPersonaAnchor CreatePersona(
        VKPersonaId id,
        string name,
        string description,
        IReadOnlyDictionary<string, string>? traits = null,
        string? directiveId = null,
        IReadOnlyDictionary<string, object>? extensions = null,
        VKTenantId? tenantId = null)
    {
        VKGuard.NotNull(name);
        VKGuard.NotNull(description);

        return new VKPersonaAnchor
        {
            TenantId = tenantId ?? _identityContext.TenantId,
            Id = id,
            Name = name,
            Description = description,
            Traits = traits ?? new Dictionary<string, string>(),
            DirectiveId = directiveId,
            Extensions = extensions ?? new Dictionary<string, object>()
        };
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
        string? outputConstraints = null,
        VKTenantId? tenantId = null)
    {
        return new VKDirectiveCharter
        {
            TenantId = tenantId ?? _identityContext.TenantId,
            Id = id,
            Overview = overview,
            BehaviorRules = behaviorRules,
            SafetyRules = safetyRules,
            OutputConstraints = outputConstraints
        };
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
        IReadOnlyList<VKKnowledgeKey>? keys = null,
        VKTenantId? tenantId = null)
    {
        VKGuard.NotNull(segment);

        return new VKKnowledgeEntry
        {
            TenantId = tenantId ?? _identityContext.TenantId,
            Id = id,
            Segment = segment,
            TriggerType = triggerType,
            FilterLogic = filterLogic,
            XmlTag = xmlTag ?? PsycheConstants.XmlTags.Knowledge,
            Keys = keys ?? []
        };
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
        VKGuard.NotNull(segment);

        return new VKPatternEntry
        {
            Id = id,
            Segment = segment
        };
    }

    // --- Session ---

    /// <inheritdoc />
    public VKSessionThread CreateSession(
        VKPersonaId personaId,
        VKSessionMode mode = VKSessionMode.Isolated,
        VKSessionId? parentSessionId = null,
        VKSessionId? forkSourceSessionId = null,
        string? forkPointRef = null)
    {
        return CreateSession(new VKSessionId(_guidGenerator.Create()), personaId, mode, parentSessionId, forkSourceSessionId, forkPointRef);
    }

    /// <inheritdoc />
    public VKSessionThread CreateSession(
        VKSessionId id,
        VKPersonaId personaId,
        VKSessionMode mode = VKSessionMode.Isolated,
        VKSessionId? parentSessionId = null,
        VKSessionId? forkSourceSessionId = null,
        string? forkPointRef = null,
        VKSessionStatus status = VKSessionStatus.Active,
        int turnCount = 0,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null,
        DateTimeOffset? lastActivityAt = null,
        VKTenantId? tenantId = null,
        VKUserId? userId = null,
        VKSessionKnowledgeState? knowledgeState = null)
    {
        var now = _timeProvider.GetUtcNow();

        return new VKSessionThread
        {
            TenantId = tenantId ?? _identityContext.TenantId,
            UserId = userId ?? _identityContext.UserId,
            Id = id,
            PersonaId = personaId,
            Mode = mode,
            ParentSessionId = parentSessionId,
            ForkSourceSessionId = forkSourceSessionId,
            ForkPointRef = forkPointRef,
            Status = status,
            TurnCount = turnCount,
            CreatedAt = createdAt ?? now,
            UpdatedAt = updatedAt ?? now,
            LastActivityAt = lastActivityAt,
            KnowledgeState = knowledgeState ?? new VKSessionKnowledgeState()
        };
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
        DateTimeOffset? createdAt = null,
        VKTenantId? tenantId = null)
    {
        VKGuard.NotNull(content);

        return new VKEchoTrace
        {
            TenantId = tenantId ?? _identityContext.TenantId,
            SessionId = sessionId,
            Id = id,
            Role = role,
            Content = content,
            TokenCount = tokenCount,
            CreatedAt = createdAt ?? _timeProvider.GetUtcNow()
        };
    }
}
