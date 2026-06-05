using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Echo;

namespace VK.Labs.PersonaWeavePulsar.Controllers;

[ApiController]
[Route("api/pwp/chat")]
public sealed class ChatController(
    PwpChatEngine chatEngine,
    IPwpChatHistoryStore historyStore) : ControllerBase
{
    private readonly PwpChatEngine _chatEngine = VKGuard.NotNull(chatEngine);
    private readonly IPwpChatHistoryStore _historyStore = VKGuard.NotNull(historyStore);

    [HttpPost("send")]
    public async Task<IActionResult> SendMessageAsync([FromBody] PwpSendMessageRequest request, CancellationToken ct)
    {
        VKGuard.NotNull(request);

        var result = await _chatEngine.SendMessageAsync(
            request.SessionId,
            request.PersonaId,
            request.Message,
            new PwpChatArgs
            {
                Temperature = request.Temperature ?? 0.9f,
            },
            ct).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return Ok(new
        {
            SessionId = request.SessionId,
            Response = result.Value
        });
    }

    [HttpGet("history/{sessionId}")]
    public async Task<IActionResult> GetHistoryAsync(string sessionId, [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var result = await _historyStore.GetHistoryAsync(sessionId, limit, ct).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return Ok(result.Value);
    }

    [HttpDelete("history/{sessionId}")]
    public async Task<IActionResult> ClearHistoryAsync(string sessionId, CancellationToken ct = default)
    {
        var result = await _historyStore.ClearHistoryAsync(sessionId, ct).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return BadRequest(result.FirstError.Description);
        }

        return NoContent();
    }

    [HttpPut("{sessionId}/messages/{messageId}")]
    public async Task<IActionResult> UpdateMessageAsync(string sessionId, string messageId, [FromBody] UpdateMessageRequest request, CancellationToken ct)
    {
        VKGuard.NotNull(request);
        var result = await _historyStore.UpdateMessageAsync(sessionId, messageId, request.Content, ct).ConfigureAwait(false);
        if (result.IsFailure)
            return BadRequest(result.FirstError.Description);
        return Ok();
    }

    [HttpDelete("{sessionId}/messages/{messageId}")]
    public async Task<IActionResult> DeleteMessageAsync(string sessionId, string messageId, CancellationToken ct)
    {
        var result = await _historyStore.DeleteMessageAsync(sessionId, messageId, ct).ConfigureAwait(false);
        if (result.IsFailure)
            return BadRequest(result.FirstError.Description);
        return NoContent();
    }

    [HttpPost("regenerate")]
    public async Task<IActionResult> RegenerateAsync([FromBody] PwpRegenerateRequest request, CancellationToken ct)
    {
        VKGuard.NotNull(request);
        var result = await _chatEngine.RegenerateAsync(request.SessionId, request.PersonaId, null, ct).ConfigureAwait(false);
        if (result.IsFailure)
            return BadRequest(result.FirstError.Description);
        return Ok(new { SessionId = request.SessionId, Response = result.Value });
    }
}

public sealed record PwpSendMessageRequest
{
    public required string SessionId { get; init; }
    public required string PersonaId { get; init; }
    public required string Message { get; init; }
    public float? Temperature { get; init; }
    public int? MaxTokens { get; init; }
}

public sealed record UpdateMessageRequest
{
    public required string Content { get; init; }
}

public sealed record PwpRegenerateRequest
{
    public required string SessionId { get; init; }
    public required string PersonaId { get; init; }
}
