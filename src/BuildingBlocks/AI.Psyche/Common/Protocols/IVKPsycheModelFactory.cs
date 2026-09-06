using System;
using System.Collections.Generic;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain model factory for creating AI.Psyche entities and value objects automatically bound to ambient identity context, IVKGuidGenerator, and TimeProvider.
/// Follows AP.01, AP.03, CS.06.
/// </summary>
public interface IVKPsycheModelFactory
{
    // --- Segment & Key (Value Objects) ---

    /// <summary>
    /// Creates a new <see cref="VKPromptSegment"/> with validated layout coordinates and prompt payload.
    /// </summary>
    VKPromptSegment CreateSegment(
        string content,
        string? name = null,
        bool isEnabled = true,
        VKChatRole role = VKChatRole.System,
        int? absoluteDepth = null,
        VKPromptRelativeDepth? relativeDepth = null,
        int depthPriority = 0);

    /// <summary>
    /// Creates a new <see cref="VKKnowledgeKey"/> for knowledge trigger matching.
    /// </summary>
    VKKnowledgeKey CreateKey(
        string text,
        VKKnowledgeMatchType matchType = VKKnowledgeMatchType.Contains,
        bool caseSensitive = false);

    // --- Persona ---

    /// <summary>
    /// Creates a new <see cref="VKPersonaAnchor"/> with an automatically generated ID.
    /// </summary>
    VKPersonaAnchor CreatePersona(
        string name,
        string description,
        IReadOnlyDictionary<string, string>? traits = null,
        IReadOnlyDictionary<string, object>? extensions = null);

    /// <summary>
    /// Creates a new <see cref="VKPersonaAnchor"/> with an explicitly specified ID.
    /// </summary>
    VKPersonaAnchor CreatePersona(
        VKPersonaId id,
        string name,
        string description,
        IReadOnlyDictionary<string, string>? traits = null,
        IReadOnlyDictionary<string, object>? extensions = null);

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
        string? outputConstraints = null);

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
        IReadOnlyList<VKKnowledgeKey>? keys = null);

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
        VKSessionMode mode = VKSessionMode.Isolated,
        VKSessionId? parentSessionId = null,
        VKSessionId? forkSourceSessionId = null,
        string? forkPointRef = null,
        VKSessionKnowledgeState? knowledgeState = null);

    /// <summary>
    /// Creates a new <see cref="VKSessionThread"/> with an explicitly specified ID.
    /// </summary>
    VKSessionThread CreateSession(
        VKSessionId id,
        VKSessionMode mode = VKSessionMode.Isolated,
        VKSessionId? parentSessionId = null,
        VKSessionId? forkSourceSessionId = null,
        string? forkPointRef = null,
        VKSessionKnowledgeState? knowledgeState = null);

    // --- Profile ---

    /// <summary>
    /// Creates a new <see cref="VKProfilePresence"/> with an automatically generated ID.
    /// </summary>
    VKProfilePresence CreateProfile(
        string? displayName = null,
        string? preferredLanguage = null,
        string? timeZone = null,
        IReadOnlyDictionary<string, string>? preferences = null);

    /// <summary>
    /// Creates a new <see cref="VKProfilePresence"/> with an explicitly specified ID.
    /// </summary>
    VKProfilePresence CreateProfile(
        VKProfileId id,
        string? displayName = null,
        string? preferredLanguage = null,
        string? timeZone = null,
        IReadOnlyDictionary<string, string>? preferences = null);

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
        DateTimeOffset? createdAt = null);
}
