using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain persistence port for <see cref="VKUser"/> aggregate root.
/// Inherits full CRUD and batch resolution capabilities from <see cref="IVKAggregateRepository{TAggregate, TId}"/>.
/// Follows AP.01, CS.01, and CS.03.
/// </summary>
public interface IVKIdentityUserRepository : IVKAggregateRepository<VKUser, VKUserId>
{
}
