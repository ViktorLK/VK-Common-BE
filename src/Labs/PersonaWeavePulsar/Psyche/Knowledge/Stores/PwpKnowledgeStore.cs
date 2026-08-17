using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using VK.Labs.PersonaWeavePulsar.Features.KnowledgeBook.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Diagnostics;
using VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Stores;

/// <summary>
/// SQLite implementation of Psyche's <see cref="IVKKnowledgeStore"/>.
/// Focuses purely on AI runtime knowledge retrieval for the Psyche pipeline.
/// </summary>
internal sealed class PwpKnowledgeStore : IVKKnowledgeStore
{
    private readonly IVKReadRepository<PwpKnowledgeEntity> _entryRepository;
    private readonly IVKReadRepository<PwpKnowledgeBookEntity> _bookRepository;
    private readonly IVKReadRepository<PwpPersonaKnowledgeBookEntity> _personaBookRepository;
    private readonly IVKPsycheModelFactory _modelFactory;
    private readonly ILogger<PwpKnowledgeStore> _logger;

    public PwpKnowledgeStore(
        IVKReadRepository<PwpKnowledgeEntity> entryRepository,
        IVKReadRepository<PwpKnowledgeBookEntity> bookRepository,
        IVKReadRepository<PwpPersonaKnowledgeBookEntity> personaBookRepository,
        IVKPsycheModelFactory modelFactory,
        ILogger<PwpKnowledgeStore> logger)
    {
        _entryRepository = VKGuard.NotNull(entryRepository);
        _bookRepository = VKGuard.NotNull(bookRepository);
        _personaBookRepository = VKGuard.NotNull(personaBookRepository);
        _modelFactory = VKGuard.NotNull(modelFactory);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<IEnumerable<VKKnowledgeEntry>>> GetRelevantEntriesAsync(
        VKPersonaId personaId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(personaId);

        try
        {
            var bookIds = await GetRelevantBookIdsAsync(personaId, cancellationToken).ConfigureAwait(false);
            var entities = await LoadEntriesWithKeysAsync(
                e => e.Segment.IsEnabled && bookIds.Contains(e.KnowledgeBookId),
                cancellationToken).ConfigureAwait(false);

            return VKResult.Success(entities.Select(MapToDomain));
        }
        catch (Exception ex)
        {
            _logger.LogGetRelevantKnowledgeError(ex, personaId.ToString());
            return VKResult.Failure<IEnumerable<VKKnowledgeEntry>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    private async Task<HashSet<PwpKnowledgeBookId>> GetRelevantBookIdsAsync(VKPersonaId personaId, CancellationToken cancellationToken)
    {
        var personaBooks = await _personaBookRepository
            .GetListAsync(pb => pb.PersonaId == personaId, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var globalBooks = await _bookRepository
            .GetListAsync(b => b.IsGlobal, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var bookIds = new HashSet<PwpKnowledgeBookId>();
        foreach (var pb in personaBooks)
        {
            bookIds.Add(pb.KnowledgeBookId);
        }
        foreach (var book in globalBooks)
        {
            bookIds.Add(book.Id);
        }
        return bookIds;
    }

    private async Task<IReadOnlyList<PwpKnowledgeEntity>> LoadEntriesWithKeysAsync(
        Expression<Func<PwpKnowledgeEntity, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return await _entryRepository.GetListAsync(
            predicate,
            q => q.Include(e => e.Keys),
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private VKKnowledgeEntry MapToDomain(PwpKnowledgeEntity entity)
    {
        return _modelFactory.CreateKnowledge(
            entity.Id,
            entity.Segment.ToDomainSegment(),
            entity.TriggerType,
            entity.FilterLogic,
            entity.Tag,
            [.. entity.Keys.Select(MapKeyToDomain)]);
    }

    private static VKKnowledgeKey MapKeyToDomain(PwpKnowledgeKeyEntity key)
    {
        return new VKKnowledgeKey
        {
            Text = key.Text,
            MatchType = key.MatchType,
            CaseSensitive = key.CaseSensitive
        };
    }
}
