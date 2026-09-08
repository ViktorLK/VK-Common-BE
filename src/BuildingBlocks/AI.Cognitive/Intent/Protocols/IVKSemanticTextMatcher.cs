using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Domain contract for calculating semantic similarity or intent matching between texts.
/// </summary>
public interface IVKSemanticTextMatcher
{
    /// <summary>
    /// Computes similarity score between input text and candidate pattern (0.0 to 1.0).
    /// </summary>
    Task<double> ComputeSimilarityAsync(string input, string pattern, CancellationToken ct = default);
}
