using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

internal sealed class DefaultMemoryPipelineStage : IVKOrchestrationPipelineStage
{
    private readonly VKMemoryOptions _options;
    private readonly IVKMemoryEchoes _echoes;

    public int Order => 400;

    public bool IsActive => _options.Enabled;

    public bool IsParallel => false;

    public int? ParallelGroup => null;

    public DefaultMemoryPipelineStage(IOptions<VKMemoryOptions> options, IVKMemoryEchoes echoes)
    {
        _options = VKGuard.NotNull(options).Value;
        _echoes = VKGuard.NotNull(echoes);
    }

    public async Task ExecuteAsync(VKOrchestrationPipelineContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        // Fetch ShortTerm memory (ChatHistory) autonomously if not provided
        if (context.Messages == null && !string.IsNullOrWhiteSpace(context.SessionId))
        {
            var limit = 50;
            if (context.Args?.Context.TryGetValue("MaxHistoryMessages", out var limitObj) == true && limitObj is int l)
            {
                limit = l;
            }

            var recentResult = await _echoes.GetRecentAsync(context.SessionId, VKMemoryCategory.ShortTerm, limit, ct).ConfigureAwait(false);
            
            if (recentResult.IsSuccess && recentResult.Value.Count > 0)
            {
                var messages = new System.Collections.Generic.List<VKChatMessage>();
                foreach (var entry in recentResult.Value)
                {
                    // Attempt to resolve role from metadata, fallback to Assistant
                    var roleString = entry.Metadata.TryGetValue("Role", out var r) ? r : "Assistant";
                    var role = roleString.Equals("user", System.StringComparison.OrdinalIgnoreCase) 
                        ? VKChatRole.User 
                        : (roleString.Equals("system", System.StringComparison.OrdinalIgnoreCase) ? VKChatRole.System : VKChatRole.Assistant);

                    messages.Add(new VKChatMessage
                    {
                        Role = role,
                        Content = entry.Content
                    });
                }
                context.Messages = messages;
            }
        }
    }
}
