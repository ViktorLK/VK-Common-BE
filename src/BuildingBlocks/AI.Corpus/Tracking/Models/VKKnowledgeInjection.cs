using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Represents a record of a knowledge entry injected into a psyche session.
/// </summary>
/// <param name="KnowledgeId">The unique identifier of the injected knowledge entry.</param>
/// <param name="InjectedTurn">The dialogue turn number when the injection occurred.</param>
/// <param name="GroupId">The group identifier of the injected entry, or empty if none.</param>
public sealed record VKKnowledgeInjection(VKKnowledgeId KnowledgeId, int InjectedTurn, string GroupId);
