using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.AI.Psyche.EFCore.Profile.Internal;

/// <summary>
/// EFCore implementation of Psyche's <see cref="IVKProfileStore"/>.
/// Focuses purely on AI runtime profile retrieval for the Psyche pipeline.
/// </summary>
internal sealed class ProfileStore(
    IVKEntityReadRepository<VKPsycheProfileEntity> repository,
    ILogger<ProfileStore> logger) : IVKProfileStore
{
    private readonly IVKEntityReadRepository<VKPsycheProfileEntity> _repository = VKGuard.NotNull(repository);
    private readonly ILogger<ProfileStore> _logger = VKGuard.NotNull(logger);

    public async Task<VKResult<VKProfilePresence?>> GetProfileAsync(
        VKProfileId profileId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (profileId.IsEmpty)
        {
            return VKResult.Success<VKProfilePresence?>(null);
        }

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(
                e => e.Id == profileId,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return VKResult.Success(entity?.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogGetProfileError(ex, profileId.ToString());
            return VKResult.Failure<VKProfilePresence?>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
