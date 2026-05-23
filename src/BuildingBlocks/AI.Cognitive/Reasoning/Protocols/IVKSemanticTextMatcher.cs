using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Provides semantic text similarity and embedding distance calculations.
/// </summary>
public interface IVKSemanticTextMatcher
{
    /// <summary>
    /// Calculates the semantic similarity between two text sequences, typically returning a value between 0.0 and 1.0.
    /// </summary>
    ValueTask<VKResult<double>> CalculateSimilarityAsync(string text1, string text2, CancellationToken cancellationToken = default);
}
