using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a chat session with state.
/// </summary>
public sealed class VKChatSession(IVKChatEngine engine)
{
    private readonly IVKChatEngine _engine = VKGuard.NotNull(engine);
    private readonly List<VKChatMessage> _history = [];

    /// <summary>
    /// Gets the message history.
    /// </summary>
    public IReadOnlyList<VKChatMessage> History => _history;

    /// <summary>
    /// Adds a message to the history.
    /// </summary>
    /// <param name="message">The message to add.</param>
    public void AddMessage(VKChatMessage message) => _history.Add(message);

    /// <summary>
    /// Clears the history.
    /// </summary>
    public void Clear() => _history.Clear();

    /// <summary>
    /// Sends the current history to the engine and adds the response to history.
    /// </summary>
    /// <param name="args">The execution arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the assistant response.</returns>
    public async Task<VKResult<VKChatMessage>> ChatAsync(
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _engine.SendAsync(_history, args, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            _history.Add(result.Value);
        }

        return result;
    }
}
