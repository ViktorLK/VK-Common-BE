namespace VK.Blocks.AI;

/// <summary>
/// Represents a single re-ranked result.
/// </summary>
public record VKReRankingResult(string Content, float Score, int OriginalIndex);
