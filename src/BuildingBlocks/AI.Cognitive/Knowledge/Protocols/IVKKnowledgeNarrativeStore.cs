using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.AI.Cognitive;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the storage contract for retrieving and saving narrative rules associated with knowledge entries.
/// </summary>
public interface IVKKnowledgeNarrativeStore
{
    /// <summary>
    /// Retrieves the narrative rules for a specific knowledge entry ID.
    /// </summary>
    Task<VKResult<VKKnowledgeNarrativeRules?>> GetRulesAsync(
        string knowledgeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates the narrative rules for a knowledge entry.
    /// </summary>
    Task<VKResult> SaveRulesAsync(
        VKKnowledgeNarrativeRules rules,
        CancellationToken cancellationToken = default);
}
