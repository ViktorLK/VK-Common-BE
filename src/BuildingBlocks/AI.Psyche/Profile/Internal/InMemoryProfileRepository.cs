using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Profile.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKPsycheProfileRepository"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemoryProfileRepository : VKInMemoryAggregateRepository<VKProfilePresence, VKProfileId>, IVKPsycheProfileRepository
{
    public InMemoryProfileRepository()
    {
    }

    protected override VKError GetNotFoundError(VKProfileId id) => VKProfileErrors.NotFound;

    protected override VKError GetAlreadyExistsError(VKProfileId id) => VKProfileErrors.AlreadyExists;
}
