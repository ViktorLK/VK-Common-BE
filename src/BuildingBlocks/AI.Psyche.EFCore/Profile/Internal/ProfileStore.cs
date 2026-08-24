using System;
using System.Collections.Generic;
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
internal sealed class ProfileStore : IVKProfileStore
{
    private readonly IVKReadRepository<VKPsycheProfileEntity> _repository;
    private readonly IVKJsonSerializer _serializer;
    private readonly IVKPsycheModelFactory _modelFactory;
    private readonly ILogger<ProfileStore> _logger;

    public ProfileStore(
        IVKReadRepository<VKPsycheProfileEntity> repository,
        IVKJsonSerializer serializer,
        IVKPsycheModelFactory modelFactory,
        ILogger<ProfileStore> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _serializer = VKGuard.NotNull(serializer);
        _modelFactory = VKGuard.NotNull(modelFactory);
        _logger = VKGuard.NotNull(logger);
    }

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

            if (entity is null)
            {
                return VKResult.Success<VKProfilePresence?>(null);
            }

            return VKResult.Success<VKProfilePresence?>(MapToDomain(entity));
        }
        catch (Exception ex)
        {
            _logger.LogGetProfileError(ex, profileId.ToString());
            return VKResult.Failure<VKProfilePresence?>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    private VKProfilePresence MapToDomain(VKPsycheProfileEntity entity)
    {
        var prefs = !string.IsNullOrWhiteSpace(entity.PreferencesJson)
            ? _serializer.DeserializeOrDefault<Dictionary<string, string>>(entity.PreferencesJson, [])
            : null;

        return _modelFactory.CreateProfile(
            entity.Id,
            entity.DisplayName,
            entity.PreferredLanguage,
            entity.TimeZone,
            prefs);
    }
}
