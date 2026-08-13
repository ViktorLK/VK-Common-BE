using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain model factory for creating AI.Psyche entities automatically bound to ambient identity context, IVKGuidGenerator, and TimeProvider.
/// Follows AP.01, AP.03, CS.06.
/// </summary>
public interface IVKPsycheModelFactory
{
    // --- Persona ---

    /// <summary>
    /// Creates a new <see cref="VKPersonaAnchor"/> with an automatically generated ID.
    /// </summary>
    VKPersonaAnchor CreatePersona(
        string name,
        string description,
        IReadOnlyDictionary<string, string>? traits = null,
        string? directiveId = null,
        IReadOnlyDictionary<string, object>? extensions = null);

    /// <summary>
    /// Creates a new <see cref="VKPersonaAnchor"/> with an explicitly specified ID.
    /// </summary>
    VKPersonaAnchor CreatePersona(
        VKPersonaId id,
        string name,
        string description,
        IReadOnlyDictionary<string, string>? traits = null,
        string? directiveId = null,
        IReadOnlyDictionary<string, object>? extensions = null,
        VKTenantId? tenantId = null);

    // --- Directive ---

    /// <summary>
    /// Creates a new <see cref="VKDirectiveCharter"/> with an automatically generated ID.
    /// </summary>
    VKDirectiveCharter CreateDirective(
        string? overview = null,
        string? behaviorRules = null,
        string? safetyRules = null,
        string? outputConstraints = null);

    /// <summary>
    /// Creates a new <see cref="VKDirectiveCharter"/> with an explicitly specified ID.
    /// </summary>
    VKDirectiveCharter CreateDirective(
        VKDirectiveId id,
        string? overview = null,
        string? behaviorRules = null,
        string? safetyRules = null,
        string? outputConstraints = null,
        VKTenantId? tenantId = null);

    // --- Knowledge ---

    /// <summary>
    /// Creates a new <see cref="VKKnowledgeEntry"/> with an automatically generated ID.
    /// </summary>
    VKKnowledgeEntry CreateKnowledge(
        VKPromptSegment segment,
        VKKnowledgeTriggerType triggerType = VKKnowledgeTriggerType.Constant,
        VKKnowledgeFilterLogic filterLogic = VKKnowledgeFilterLogic.AndAny,
        string? xmlTag = null,
        IReadOnlyList<VKKnowledgeKey>? keys = null);

    /// <summary>
    /// Creates a new <see cref="VKKnowledgeEntry"/> with an explicitly specified ID.
    /// </summary>
    VKKnowledgeEntry CreateKnowledge(
        VKKnowledgeId id,
        VKPromptSegment segment,
        VKKnowledgeTriggerType triggerType = VKKnowledgeTriggerType.Constant,
        VKKnowledgeFilterLogic filterLogic = VKKnowledgeFilterLogic.AndAny,
        string? xmlTag = null,
        IReadOnlyList<VKKnowledgeKey>? keys = null,
        VKTenantId? tenantId = null);

    // --- Pattern ---

    /// <summary>
    /// Creates a new <see cref="VKPatternEntry"/> with an automatically generated ID.
    /// </summary>
    VKPatternEntry CreatePattern(VKPromptSegment segment);

    /// <summary>
    /// Creates a new <see cref="VKPatternEntry"/> with an explicitly specified ID.
    /// </summary>
    VKPatternEntry CreatePattern(VKPatternId id, VKPromptSegment segment);

    // --- Session ---

    /// <summary>
    /// Creates a new <see cref="VKSessionThread"/> with an automatically generated ID.
    /// </summary>
    VKSessionThread CreateSession(
        VKPersonaId personaId,
        VKSessionMode mode = VKSessionMode.Isolated,
        VKSessionId? parentSessionId = null,
        VKSessionId? forkSourceSessionId = null,
        string? forkPointRef = null);

    /// <summary>
    /// Creates a new <see cref="VKSessionThread"/> with an explicitly specified ID and optional hydration state.
    /// </summary>
    VKSessionThread CreateSession(
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
        VKSessionKnowledgeState? knowledgeState = null);

    // --- Echo ---

    /// <summary>
    /// Creates a new <see cref="VKEchoTrace"/> with an automatically generated ID.
    /// </summary>
    VKEchoTrace CreateEcho(
        VKSessionId sessionId,
        VKChatRole role,
        string content,
        int tokenCount = 0,
        DateTimeOffset? createdAt = null);

    /// <summary>
    /// Creates a new <see cref="VKEchoTrace"/> with an explicitly specified ID.
    /// </summary>
    VKEchoTrace CreateEcho(
        VKEchoId id,
        VKSessionId sessionId,
        VKChatRole role,
        string content,
        int tokenCount = 0,
        DateTimeOffset? createdAt = null,
        VKTenantId? tenantId = null);
}
