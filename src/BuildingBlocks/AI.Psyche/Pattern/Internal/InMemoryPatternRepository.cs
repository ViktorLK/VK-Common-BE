using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pattern.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKPsychePatternRepository"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemoryPatternRepository : VKInMemoryAggregateRepository<VKPatternEntry, VKPatternId>, IVKPsychePatternRepository
{
    public InMemoryPatternRepository(IEnumerable<VKPatternEntry>? initial = null) : base(initial)
    {
    }

    protected override VKError GetNotFoundError(VKPatternId id) => VKPatternErrors.NotFound;

    protected override VKError GetAlreadyExistsError(VKPatternId id) => VKPatternErrors.AlreadyExists;
}
