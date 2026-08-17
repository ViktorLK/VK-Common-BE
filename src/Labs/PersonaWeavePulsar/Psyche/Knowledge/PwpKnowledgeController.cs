using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VK.Blocks.AI;
using VK.Blocks.AI.Corpus;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Common.Internal;
using VK.Labs.PersonaWeavePulsar.Features.KnowledgeBook.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Contracts;
using VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Repositories;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Knowledge;

/// <summary>
/// Slice controller for managing Knowledge Entries in Psyche.
/// Follows standard Repository pattern and Request/Response contract DTOs.
/// </summary>
[ApiController]
[Route("api/pwp/knowledge")]
public sealed class PwpKnowledgeController(
    IPwpKnowledgeRepository knowledgeRepository,
    IVKGuidGenerator guidGenerator) : ControllerBase
{
    private readonly IPwpKnowledgeRepository _knowledgeRepository = VKGuard.NotNull(knowledgeRepository);
    private readonly IVKGuidGenerator _guidGenerator = VKGuard.NotNull(guidGenerator);

    /// <summary>
    /// Lists all entries in a knowledge book.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetEntriesAsync([FromQuery] string? bookId, CancellationToken ct = default)
    {
        PwpKnowledgeBookId? typedBookId = !string.IsNullOrWhiteSpace(bookId) ? PwpKnowledgeBookId.Parse(bookId, null) : null;
        var result = await _knowledgeRepository.GetListAsync(typedBookId, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        var responses = result.Value.Select(MapToResponse).ToList();
        return Ok(responses);
    }

    /// <summary>
    /// Gets a specific knowledge entry by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEntryAsync([FromRoute] string id, CancellationToken ct = default)
    {
        var entryId = VKKnowledgeId.Parse(id, null);
        var result = await _knowledgeRepository.GetByIdAsync(entryId, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return NotFound(result.FirstError.Description);
        }

        return Ok(MapToResponse(result.Value));
    }

    /// <summary>
    /// Creates a new knowledge entry.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateEntryAsync([FromBody] CreateKnowledgeRequest request, CancellationToken ct = default)
    {
        VKGuard.NotNull(request);

        var entryId = VKKnowledgeId.Parse(_guidGenerator.Create().ToString("N"), null);
        var entity = MapToEntity(entryId, request);

        var result = await _knowledgeRepository.CreateAsync(entity, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return CreatedAtAction(nameof(GetEntryAsync), new { id = entryId.ToString() }, MapToResponse(entity));
    }

    /// <summary>
    /// Updates an existing knowledge entry.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEntryAsync([FromRoute] string id, [FromBody] UpdateKnowledgeRequest request, CancellationToken ct = default)
    {
        VKGuard.NotNull(request);
        var entryId = VKKnowledgeId.Parse(id, null);

        var createReq = new CreateKnowledgeRequest
        {
            BookId = request.BookId,
            Memo = request.Memo,
            Content = request.Content,
            Strategy = request.Strategy,
            Priority = request.Priority,
            Position = request.Position,
            Depth = request.Depth,
            Role = request.Role,
            Probability = request.Probability,
            InclusionGroups = request.InclusionGroups,
            GroupWeight = request.GroupWeight,
            TimedSticky = request.TimedSticky,
            TimedCooldown = request.TimedCooldown,
            TimedDelay = request.TimedDelay,
            IsEnabled = request.IsEnabled,
            Tag = request.Tag,
            Keys = request.Keys
        };

        var entity = MapToEntity(entryId, createReq);
        var result = await _knowledgeRepository.UpdateAsync(entity, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return Ok();
    }

    /// <summary>
    /// Deletes a knowledge entry.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntryAsync([FromRoute] string id, CancellationToken ct = default)
    {
        var entryId = VKKnowledgeId.Parse(id, null);
        var result = await _knowledgeRepository.DeleteAsync(entryId, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return NoContent();
    }

    private static PwpKnowledgeEntity MapToEntity(VKKnowledgeId entryId, CreateKnowledgeRequest request)
    {
        VKPromptRelativeDepth? anchor = null;
        if (!string.IsNullOrEmpty(request.Position) && Enum.TryParse<VKPromptRelativeDepth>(request.Position, true, out var rel))
        {
            anchor = rel;
        }

        return new PwpKnowledgeEntity
        {
            Id = entryId,
            KnowledgeBookId = !string.IsNullOrWhiteSpace(request.BookId) ? PwpKnowledgeBookId.Parse(request.BookId, null) : new PwpKnowledgeBookId(Guid.Empty),
            TriggerType = request.Strategy == 0 ? VKKnowledgeTriggerType.Constant : VKKnowledgeTriggerType.Keyword,
            FilterLogic = VKKnowledgeFilterLogic.AndAny,
            StickyTurns = request.TimedSticky ?? 0,
            CooldownTurns = request.TimedCooldown ?? 0,
            DelayTurns = request.TimedDelay ?? VKKnowledgeLifecyclePresets.Delay.Immediate,
            ExclusiveGroup = request.InclusionGroups?.FirstOrDefault(),
            ExclusiveWeight = request.GroupWeight ?? 100,
            Tag = request.Tag ?? "knowledge",
            Segment = new PwpPromptSegment
            {
                Content = request.Content,
                Name = request.Memo,
                IsEnabled = request.IsEnabled ?? true,
                TargetRole = VKChatRole.System,
                RelativeAnchor = anchor,
                Priority = request.Priority ?? 0
            },
            Keys = request.Keys?.Select(k => new PwpKnowledgeKeyEntity
            {
                Id = Guid.NewGuid(),
                KnowledgeEntryId = entryId,
                Text = k.Text,
                MatchType = k.IsRegex == true ? VKKnowledgeMatchType.Regex : VKKnowledgeMatchType.Contains,
                CaseSensitive = false
            }).ToList() ?? []
        };
    }

    private static KnowledgeResponse MapToResponse(PwpKnowledgeEntity entity)
    {
        return new KnowledgeResponse
        {
            Id = entity.Id.ToString(),
            BookId = entity.KnowledgeBookId.ToString(),
            Name = entity.Segment.Name,
            Content = entity.Segment.Content,
            IsEnabled = entity.Segment.IsEnabled,
            Priority = entity.Segment.Priority,
            Tag = entity.Tag,
            Keys = entity.Keys.Select(k => k.Text).ToList()
        };
    }
}
