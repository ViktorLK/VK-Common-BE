using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain persistence port for <see cref="VKPersonaAnchor"/> aggregate root.
/// Inherits full CRUD and batch resolution capabilities from <see cref="IVKAggregateRepository{TAggregate, TId}"/>.
/// Follows AP.01, CS.01, and CS.03.
/// </summary>
public interface IVKPsychePersonaRepository : IVKAggregateRepository<VKPersonaAnchor, VKPersonaId>
{
}
