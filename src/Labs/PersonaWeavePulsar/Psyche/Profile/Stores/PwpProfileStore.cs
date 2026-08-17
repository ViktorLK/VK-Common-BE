using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using VK.Labs.PersonaWeavePulsar.Psyche.Profile.Diagnostics;
using VK.Labs.PersonaWeavePulsar.Psyche.Profile.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Profile.Stores;

/// <summary>
/// SQLite repository implementation for profile presence.
/// Follows AP.01 (sealed class default) and CS.03 (ConfigureAwait(false)).
/// </summary>
internal sealed class PwpProfileStore : IVKProfileStore
{
    private readonly IVKReadRepository<PwpProfileEntity> _presenceRepository;
    private readonly IVKIdentityContext _identityContext;
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly ILogger<PwpProfileStore> _logger;

    public PwpProfileStore(
        IVKReadRepository<PwpProfileEntity> presenceRepository,
        IVKIdentityContext identityContext,
        IVKJsonSerializer jsonSerializer,
        ILogger<PwpProfileStore> logger)
    {
        _presenceRepository = VKGuard.NotNull(presenceRepository);
        _identityContext = VKGuard.NotNull(identityContext);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKProfilePresence?>> GetProfileAsync(VKUserId userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (userId.IsEmpty)
        {
            return VKResult.Success<VKProfilePresence?>(null);
        }

        try
        {
            var presenceEntity = await _presenceRepository.GetFirstOrDefaultAsync(u => u.UserId == userId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (presenceEntity is null)
            {
                return VKResult.Success<VKProfilePresence?>(null);
            }

            return VKResult.Success<VKProfilePresence?>(MapToDomain(presenceEntity));
        }
        catch (Exception ex)
        {
            _logger.LogGetProfileError(ex, userId.ToString());
            return VKResult.Failure<VKProfilePresence?>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    private VKProfilePresence MapToDomain(PwpProfileEntity entity)
    {
        var prefsDict = _jsonSerializer.DeserializeOrDefault<Dictionary<string, string>>(entity.PreferencesJson, []);

        return new VKProfilePresence
        {
            TenantId = entity.TenantId ?? _identityContext.TenantId,
            UserId = entity.UserId,
            DisplayName = entity.DisplayName,
            PreferredLanguage = entity.PreferredLanguage,
            TimeZone = entity.TimeZone,
            Preferences = prefsDict
        };
    }
}
