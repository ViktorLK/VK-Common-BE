using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Psyche.Echo.Repositories;
using VK.Labs.PersonaWeavePulsar.Psyche.Session.Contracts;
using VK.Labs.PersonaWeavePulsar.Psyche.Session.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Session.Repositories;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Session;

/// <summary>
/// Slice controller for managing PWP chat sessions in Psyche.
/// Follows standard Repository pattern and Request/Response contract DTOs.
/// </summary>
[ApiController]
[Authorize]
[Route("api/pwp/sessions")]
public sealed class PwpSessionController(
    IPwpSessionRepository sessionRepository,
    IPwpEchoRepository echoRepository,
    IVKGuidGenerator guidGenerator) : ControllerBase
{
    private readonly IPwpSessionRepository _sessionRepository = VKGuard.NotNull(sessionRepository);
    private readonly IPwpEchoRepository _echoRepository = VKGuard.NotNull(echoRepository);
    private readonly IVKGuidGenerator _guidGenerator = VKGuard.NotNull(guidGenerator);

    /// <summary>
    /// Retrieves all chat sessions.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSessionsAsync(CancellationToken ct)
    {
        var result = await _sessionRepository.GetListAsync(ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        var responses = result.Value.Select(MapToResponse).ToList();
        return Ok(responses);
    }

    /// <summary>
    /// Gets a specific chat session by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSessionAsync([FromRoute] string id, CancellationToken ct)
    {
        var sessionId = VKSessionId.Parse(id, null);
        var result = await _sessionRepository.GetByIdAsync(sessionId, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return NotFound(result.FirstError.Description);
        }

        return Ok(MapToResponse(result.Value));
    }

    /// <summary>
    /// Creates a new chat session entity.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSessionAsync([FromBody] CreateSessionRequest request, CancellationToken ct)
    {
        VKGuard.NotNull(request);

        var now = DateTimeOffset.UtcNow;
        var entity = new PwpSessionEntity
        {
            Id = VKSessionId.Parse(_guidGenerator.Create().ToString("N"), null),
            PersonaId = VKPersonaId.Parse(request.PersonaId, null),
            CreatedAt = now,
            UpdatedAt = now,
            LastActivityAt = now,
            CustomModelId = request.ModelId,
            CustomApiKey = request.ApiKey,
            CustomEndpoint = request.Endpoint,
            CustomServiceType = request.ServiceType
        };

        var result = await _sessionRepository.CreateAsync(entity, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return CreatedAtAction(nameof(GetSessionAsync), new { id = entity.Id.ToString() }, MapToResponse(entity));
    }

    /// <summary>
    /// Deletes a chat session and its echo history.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSessionAsync([FromRoute] string id, CancellationToken ct)
    {
        var sessionId = VKSessionId.Parse(id, null);
        var result = await _sessionRepository.DeleteAsync(sessionId, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        await _echoRepository.ClearHistoryAsync(sessionId, ct).ConfigureAwait(false);
        return NoContent();
    }

    private static SessionResponse MapToResponse(PwpSessionEntity entity)
    {
        return new SessionResponse
        {
            Id = entity.Id.ToString(),
            PersonaId = entity.PersonaId.ToString(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            LastActivityAt = entity.LastActivityAt,
            ModelId = entity.CustomModelId,
            Endpoint = entity.CustomEndpoint,
            ServiceType = entity.CustomServiceType
        };
    }
}
