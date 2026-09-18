using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.IntegrationTests.Fixtures;

/// <summary>
/// Controllable test chat engine implementation for Psyche integration testing.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
public sealed class TestChatEngine : IVKChatEngine
{
    private readonly List<IReadOnlyList<VKChatMessage>> _receivedBatches = [];

    public IReadOnlyList<IReadOnlyList<VKChatMessage>> ReceivedMessageBatches
    {
        get
        {
            lock (_receivedBatches)
            {
                return _receivedBatches.ToList();
            }
        }
    }

    public string ReplyContent { get; set; } = "Integration assistant test response";

    public Task<VKResult<VKChatResponse>> SendAsync(
        IEnumerable<VKChatMessage> messages,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(messages);

        var list = messages.ToList();
        lock (_receivedBatches)
        {
            _receivedBatches.Add(list);
        }

        var response = new VKChatResponse
        {
            Message = new VKChatMessage
            {
                Role = VKChatRole.Assistant,
                Content = ReplyContent
            },
            Usage = new VKAITokenUsage
            {
                InputTokens = 15,
                OutputTokens = 25
            }
        };

        return Task.FromResult(VKResult.Success(response));
    }

    public async IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        IEnumerable<VKChatMessage> messages,
        VKChatArgs? args = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(messages);

        await Task.CompletedTask;
        yield return VKResult.Success(new VKChatStreamingResponse
        {
            Delta = ReplyContent
        });
    }

    public void ClearBatches()
    {
        lock (_receivedBatches)
        {
            _receivedBatches.Clear();
        }
    }
}
