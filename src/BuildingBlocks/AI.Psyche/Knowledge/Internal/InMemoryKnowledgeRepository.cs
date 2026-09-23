using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKPsycheKnowledgeRepository"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemoryKnowledgeRepository : VKInMemoryAggregateRepository<VKKnowledgeEntry, VKKnowledgeId>, IVKPsycheKnowledgeRepository
{
    public InMemoryKnowledgeRepository()
    {
    }

    protected override VKError GetNotFoundError(VKKnowledgeId id) => VKKnowledgeErrors.NotFound;

    protected override VKError GetAlreadyExistsError(VKKnowledgeId id) => VKKnowledgeErrors.AlreadyExists;
}
