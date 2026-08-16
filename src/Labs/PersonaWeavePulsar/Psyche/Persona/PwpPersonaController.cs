using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Psyche.Persona.Contracts;
using VK.Labs.PersonaWeavePulsar.Psyche.Persona.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Persona.Repositories;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Persona;

/// <summary>
/// Slice controller for managing AI Personas in Psyche.
/// Follows standard Repository pattern and Request/Response contract DTOs.
/// </summary>
[ApiController]
[Route("api/pwp/personas")]
public sealed class PwpPersonaController(
    IPwpPersonaRepository personaRepository,
    IVKJsonSerializer serializer,
    IVKGuidGenerator guidGenerator) : ControllerBase
{
    private readonly IPwpPersonaRepository _personaRepository = VKGuard.NotNull(personaRepository);
    private readonly IVKJsonSerializer _serializer = VKGuard.NotNull(serializer);
    private readonly IVKGuidGenerator _guidGenerator = VKGuard.NotNull(guidGenerator);

    /// <summary>
    /// Lists all available personas.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPersonasAsync(CancellationToken ct)
    {
        var result = await _personaRepository.GetListAsync(ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        var responses = result.Value.Select(MapToResponse).ToList();
        return Ok(responses);
    }

    /// <summary>
    /// Gets a specific persona by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPersonaAsync([FromRoute] string id, CancellationToken ct)
    {
        var personaId = VKPersonaId.Parse(id, null);
        var result = await _personaRepository.GetByIdAsync(personaId, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return NotFound(result.FirstError.Description);
        }

        return Ok(MapToResponse(result.Value));
    }

    /// <summary>
    /// Creates a new persona entity.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePersonaAsync([FromBody] CreatePersonaRequest request, CancellationToken ct)
    {
        VKGuard.NotNull(request);

        var traits = BuildTraitsDictionary(request.Traits, request.Personality, request.Scenario, request.FirstMessage, request.DialogueExamples, request.SystemPrompt);

        var entity = new PwpPersonaEntity
        {
            Id = VKPersonaId.Parse(_guidGenerator.Create().ToString("N"), null),
            Name = request.Name,
            Description = request.Description,
            DirectiveId = !string.IsNullOrWhiteSpace(request.DirectiveId) ? VKDirectiveId.Parse(request.DirectiveId, null) : null,
            Traits = traits.Count > 0 ? _serializer.Serialize(traits) : null
        };

        var result = await _personaRepository.CreateAsync(entity, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return CreatedAtAction(nameof(GetPersonaAsync), new { id = entity.Id.ToString() }, MapToResponse(result.Value));
    }

    /// <summary>
    /// Updates an existing persona entity.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePersonaAsync([FromRoute] string id, [FromBody] UpdatePersonaRequest request, CancellationToken ct)
    {
        VKGuard.NotNull(request);
        var personaId = VKPersonaId.Parse(id, null);

        var traits = BuildTraitsDictionary(request.Traits, request.Personality, request.Scenario, request.FirstMessage, request.DialogueExamples, request.SystemPrompt);

        var entity = new PwpPersonaEntity
        {
            Id = personaId,
            Name = request.Name,
            Description = request.Description,
            DirectiveId = !string.IsNullOrWhiteSpace(request.DirectiveId) ? VKDirectiveId.Parse(request.DirectiveId, null) : null,
            Traits = traits.Count > 0 ? _serializer.Serialize(traits) : null
        };

        var result = await _personaRepository.UpdateAsync(entity, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return Ok();
    }

    /// <summary>
    /// Deletes a persona.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePersonaAsync([FromRoute] string id, CancellationToken ct)
    {
        var personaId = VKPersonaId.Parse(id, null);
        var result = await _personaRepository.DeleteAsync(personaId, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return NoContent();
    }

    private PersonaResponse MapToResponse(PwpPersonaEntity entity)
    {
        var traits = _serializer.DeserializeOrDefault<Dictionary<string, string>>(entity.Traits, []) ?? [];
        return new PersonaResponse
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Description = entity.Description,
            DirectiveId = entity.DirectiveId?.ToString(),
            Traits = traits
        };
    }

    private static Dictionary<string, string> BuildTraitsDictionary(
        Dictionary<string, string>? baseTraits,
        string? personality,
        string? scenario,
        string? firstMessage,
        string? dialogueExamples,
        string? systemPrompt)
    {
        var traits = baseTraits is not null ? new Dictionary<string, string>(baseTraits) : [];
        if (!string.IsNullOrEmpty(personality)) traits["Personality"] = personality;
        if (!string.IsNullOrEmpty(scenario)) traits["Scenario"] = scenario;
        if (!string.IsNullOrEmpty(firstMessage)) traits["FirstMessage"] = firstMessage;
        if (!string.IsNullOrEmpty(dialogueExamples)) traits["DialogueExamples"] = dialogueExamples;
        if (!string.IsNullOrEmpty(systemPrompt)) traits["SystemPrompt"] = systemPrompt;
        return traits;
    }
}
