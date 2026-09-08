using System.Linq;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.Identity.EFCore;

/// <summary>
/// Dynamic query contributor that enforces user-level data isolation.
/// Automatically filters entities implementing <see cref="IVKUserScoped"/> by the current <see cref="IVKIdentityContext.UserId"/>
/// when user context is active and not a system background task.
/// Follows AP.01, AP.07, CS.01.
/// </summary>
public sealed class VKPersonalDataPermissionQueryContributor(
    IVKUserContext userContext) : IVKQueryContributor
{
    private readonly IVKUserContext _userContext = VKGuard.NotNull(userContext);

    /// <inheritdoc />
    public int Priority => 100;

    /// <inheritdoc />
    public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> query) where TEntity : class
    {
        // Only apply to entities implementing IVKUserScoped
        if (!typeof(IVKUserScoped).IsAssignableFrom(typeof(TEntity)))
        {
            return query;
        }

        var currentUserId = _userContext.UserId;

        // If running in System background worker or anonymous, do not restrict personal scope
        if (currentUserId == VKUserId.System || currentUserId == VKUserId.Anonymous)
        {
            return query;
        }

        // Dynamically append WHERE e.UserId == currentUserId
        return query.Cast<IVKUserScoped>()
            .Where(e => e.UserId == currentUserId)
            .Cast<TEntity>();
    }
}
