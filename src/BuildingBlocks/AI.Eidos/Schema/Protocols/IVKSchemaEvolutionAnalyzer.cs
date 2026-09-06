using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Offline evolution analyzer for replaying historical raw LLM responses against proposed schema changes.
/// </summary>
public interface IVKSchemaEvolutionAnalyzer
{
    /// <summary>
    /// Evaluates compatibility and executes offline verification of historical payloads against a target schema.
    /// </summary>
    Task<VKResult<VKSchemaEvolutionAnalysisReport>> AnalyzeEvolutionAsync(
        VKAIEidosSchema sourceSchema,
        VKAIEidosSchema targetSchema,
        IReadOnlyList<string> historicalPayloads,
        CancellationToken cancellationToken = default);
}
