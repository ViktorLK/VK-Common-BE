namespace VK.Blocks.VectorSearch.Pipeline.Internal;

/// <summary>
/// State tracking whether the semantic cache was hit during query execution.
/// </summary>
internal sealed record SemanticCacheHitState(bool IsHit);
