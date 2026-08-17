using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Corpus;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Features.KnowledgeBook.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Repositories;

/// <summary>
/// Industrial-grade repository interface for managing lorebook and knowledge entities.
/// </summary>
public interface IPwpKnowledgeRepository
{
    Task<VKResult<PwpKnowledgeEntity>> GetByIdAsync(VKKnowledgeId knowledgeId, CancellationToken cancellationToken = default);
    Task<VKResult<IEnumerable<PwpKnowledgeEntity>>> GetListAsync(PwpKnowledgeBookId? knowledgeBookId = null, CancellationToken cancellationToken = default);
    Task<VKResult> CreateAsync(PwpKnowledgeEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> UpdateAsync(PwpKnowledgeEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> DeleteAsync(VKKnowledgeId entryId, CancellationToken cancellationToken = default);
}
