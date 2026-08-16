using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Psyche.Echo.Contracts;
using VK.Labs.PersonaWeavePulsar.Psyche.Echo.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Echo.Repositories;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Echo;

/// <summary>
/// Slice controller for managing Chat Echo history and message entries in Psyche.
/// Follows standard Repository pattern and Request/Response contract DTOs.
/// </summary>
[ApiController]
[Route("api/pwp/sessions/{sessionId}/messages")]
public sealed class PwpEchoController(
    IPwpEchoRepository echoRepository) : ControllerBase
{
    private readonly IPwpEchoRepository _echoRepository = VKGuard.NotNull(echoRepository);

    [HttpGet]
    public async Task<IActionResult> GetMessagesAsync([FromRoute] string sessionId, [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var sId = VKSessionId.Parse(sessionId, null);
        var result = await _echoRepository.GetHistoryAsync(sId, limit, ct).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        var responses = result.Value.Select(MapToResponse).ToList();
        return Ok(responses);
    }

    [HttpDelete]
    public async Task<IActionResult> ClearMessagesAsync([FromRoute] string sessionId, CancellationToken ct = default)
    {
        var sId = VKSessionId.Parse(sessionId, null);
        var result = await _echoRepository.ClearHistoryAsync(sId, ct).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return NoContent();
    }

    [HttpPut("{messageId}")]
    public async Task<IActionResult> UpdateMessageAsync([FromRoute] string sessionId, [FromRoute] string messageId, [FromBody] UpdateSessionMessageRequest request, CancellationToken ct = default)
    {
        VKGuard.NotNull(request);
        var traceId = VKEchoId.Parse(messageId, null);
        var sId = VKSessionId.Parse(sessionId, null);

        var entity = new PwpEchoEntity
        {
            Id = traceId,
            SessionId = sId,
            Content = request.Content
        };

        var result = await _echoRepository.UpdateAsync(entity, ct).ConfigureAwait(false);
        if (result.IsFailure)
            return BadRequest(result.FirstError.Description);
        return Ok();
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessageAsync([FromRoute] string sessionId, [FromRoute] string messageId, CancellationToken ct = default)
    {
        var traceId = VKEchoId.Parse(messageId, null);
        var sId = VKSessionId.Parse(sessionId, null);
        var result = await _echoRepository.DeleteAsync(sId, traceId, ct).ConfigureAwait(false);
        if (result.IsFailure)
            return BadRequest(result.FirstError.Description);
        return NoContent();
    }

    private static SessionMessageResponse MapToResponse(PwpEchoEntity entity)
    {
        return new SessionMessageResponse
        {
            Id = entity.Id.ToString(),
            SessionId = entity.SessionId.ToString(),
            Role = entity.Role.ToString(),
            Content = entity.Content,
            TokenCount = entity.TokenCount,
            CreatedAt = entity.CreatedAt
        };
    }
}
