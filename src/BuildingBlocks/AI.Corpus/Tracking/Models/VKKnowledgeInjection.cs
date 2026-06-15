using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Corpus;

public sealed record VKKnowledgeInjection(VKKnowledgeId KnowledgeId, int InjectedTurn, string GroupId);
