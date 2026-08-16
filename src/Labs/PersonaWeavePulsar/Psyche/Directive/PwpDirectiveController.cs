using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Psyche.Directive.Contracts;
using VK.Labs.PersonaWeavePulsar.Psyche.Directive.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Directive.Repositories;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Directive;

/// <summary>
/// Slice controller for managing AI Directives in Psyche.
/// Follows standard Repository pattern and Request/Response contract DTOs.
/// </summary>
[ApiController]
[Route("api/pwp/directives")]
public sealed class PwpDirectiveController(
    IPwpDirectiveRepository directiveRepository,
    IVKGuidGenerator guidGenerator) : ControllerBase
{
    private readonly IPwpDirectiveRepository _directiveRepository = VKGuard.NotNull(directiveRepository);
    private readonly IVKGuidGenerator _guidGenerator = VKGuard.NotNull(guidGenerator);

    [HttpGet]
    public async Task<IActionResult> GetDirectivesAsync(CancellationToken ct)
    {
        var result = await _directiveRepository.GetListAsync(ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        var dtos = result.Value.Select(d => new DirectiveResponse
        {
            Id = d.Id.ToString(),
            BehaviorRules = d.BehaviorRules,
            SafetyRules = d.SafetyRules,
            OutputConstraints = d.OutputConstraints,
            Overview = d.Overview
        });

        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDirectiveAsync([FromRoute] string id, CancellationToken ct)
    {
        var directiveId = VKDirectiveId.Parse(id, null);
        var result = await _directiveRepository.GetByIdAsync(directiveId, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return NotFound(result.FirstError.Description);
        }

        var d = result.Value;
        return Ok(new DirectiveResponse
        {
            Id = d.Id.ToString(),
            BehaviorRules = d.BehaviorRules,
            SafetyRules = d.SafetyRules,
            OutputConstraints = d.OutputConstraints,
            Overview = d.Overview
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateDirectiveAsync([FromBody] CreateDirectiveRequest request, CancellationToken ct)
    {
        VKGuard.NotNull(request);

        var entity = new PwpDirectiveEntity
        {
            Id = VKDirectiveId.Parse(_guidGenerator.Create().ToString("N"), null),
            Overview = request.Overview,
            BehaviorRules = request.BehaviorRules,
            SafetyRules = request.SafetyRules,
            OutputConstraints = request.OutputConstraints
        };

        var result = await _directiveRepository.CreateAsync(entity, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return CreatedAtAction(nameof(GetDirectiveAsync), new { id = entity.Id.ToString() }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDirectiveAsync([FromRoute] string id, [FromBody] UpdateDirectiveRequest request, CancellationToken ct)
    {
        VKGuard.NotNull(request);

        var entity = new PwpDirectiveEntity
        {
            Id = VKDirectiveId.Parse(id, null),
            Overview = request.Overview,
            BehaviorRules = request.BehaviorRules,
            SafetyRules = request.SafetyRules,
            OutputConstraints = request.OutputConstraints
        };

        var result = await _directiveRepository.UpdateAsync(entity, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDirectiveAsync([FromRoute] string id, CancellationToken ct)
    {
        var result = await _directiveRepository.DeleteAsync(VKDirectiveId.Parse(id, null), ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return NoContent();
    }
}
