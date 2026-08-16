using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Common.Internal;
using VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Contracts;
using VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Repositories;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Pattern;

/// <summary>
/// Slice controller for managing AI Patterns in Psyche.
/// Follows standard Repository pattern and Request/Response contract DTOs.
/// </summary>
[ApiController]
[Route("api/pwp/patterns")]
public sealed class PwpPatternController(
    IPwpPatternRepository patternRepository,
    IVKGuidGenerator guidGenerator) : ControllerBase
{
    private readonly IPwpPatternRepository _patternRepository = VKGuard.NotNull(patternRepository);
    private readonly IVKGuidGenerator _guidGenerator = VKGuard.NotNull(guidGenerator);

    [HttpGet]
    public async Task<IActionResult> GetPatternsAsync(CancellationToken ct)
    {
        var result = await _patternRepository.GetListAsync(ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        var responses = result.Value.Select(MapToResponse).ToList();
        return Ok(responses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatternAsync([FromRoute] string id, CancellationToken ct)
    {
        var patternId = VKPatternId.Parse(id, null);
        var result = await _patternRepository.GetByIdAsync(patternId, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return NotFound(result.FirstError.Description);
        }

        return Ok(MapToResponse(result.Value));
    }

    [HttpPost]
    public async Task<IActionResult> CreatePatternAsync([FromBody] CreatePatternRequest request, CancellationToken ct)
    {
        VKGuard.NotNull(request);

        var entity = new PwpPatternEntity
        {
            Id = VKPatternId.Parse(_guidGenerator.Create().ToString("N"), null),
            Segment = new PwpPromptSegment
            {
                Content = request.Content ?? string.Empty,
                Name = request.Name ?? string.Empty,
                IsEnabled = true,
                TargetRole = (VKChatRole)(request.TargetRole ?? 0),
                AbsoluteDepth = request.AbsoluteDepth,
                RelativeAnchor = (VKPromptRelativeDepth?)request.RelativeAnchor,
                Priority = request.Priority ?? 0
            }
        };

        var result = await _patternRepository.CreateAsync(entity, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return CreatedAtAction(nameof(GetPatternAsync), new { id = entity.Id.ToString() }, MapToResponse(entity));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePatternAsync([FromRoute] string id, [FromBody] UpdatePatternRequest request, CancellationToken ct)
    {
        VKGuard.NotNull(request);

        var entity = new PwpPatternEntity
        {
            Id = VKPatternId.Parse(id, null),
            Segment = new PwpPromptSegment
            {
                Content = request.Content ?? string.Empty,
                Name = request.Name ?? string.Empty,
                IsEnabled = true,
                TargetRole = (VKChatRole)(request.TargetRole ?? 0),
                AbsoluteDepth = request.AbsoluteDepth,
                RelativeAnchor = (VKPromptRelativeDepth?)request.RelativeAnchor,
                Priority = request.Priority ?? 0
            }
        };

        var result = await _patternRepository.UpdateAsync(entity, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePatternAsync([FromRoute] string id, CancellationToken ct)
    {
        var result = await _patternRepository.DeleteAsync(VKPatternId.Parse(id, null), ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return NoContent();
    }

    private static PatternResponse MapToResponse(PwpPatternEntity p)
    {
        return new PatternResponse
        {
            Id = p.Id.Value.ToString(),
            Name = p.Segment.Name ?? string.Empty,
            Content = p.Segment.Content,
            TargetRole = (int)p.Segment.TargetRole,
            AbsoluteDepth = p.Segment.AbsoluteDepth,
            RelativeAnchor = (int?)p.Segment.RelativeAnchor,
            Priority = p.Segment.Priority
        };
    }
}
