using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Agents.Internal;

/// <summary>
/// Semantic Kernel implementation of <see cref="IVKAgent"/>.
/// </summary>
internal sealed class AISKAgent : AISKProviderBase, IVKAgent
{
    private readonly VKAgentsOptions _options;

    internal ChatCompletionAgent InnerAgent { get; }

    public AISKAgent(
        Microsoft.SemanticKernel.Kernel kernel,
        string modelName,
        string name,
        string description,
        string instructions,
        VKAgentsOptions options,
        IEnumerable<IVKAtomicTool>? tools = null,
        IReadOnlyDictionary<string, object>? metadata = null)
        : base(kernel, modelName)
    {
        Name = VKGuard.NotNull(name);
        Description = VKGuard.NotNull(description);
        Instructions = instructions ?? string.Empty;
        _options = VKGuard.NotNull(options);
        Tools = tools?.ToArray() ?? [];
        Metadata = metadata ?? new Dictionary<string, object>();

        var agentKernel = kernel.Clone();

        if (Tools.Count > 0)
        {
            var functions = Tools.Select(AISKAgentToolAdapter.ToKernelFunction).ToArray();
            var plugin = KernelPluginFactory.CreateFromFunctions("AgentTools", functions);
            agentKernel.Plugins.Add(plugin);
        }

        InnerAgent = new ChatCompletionAgent
        {
            Name = Name,
            Description = Description,
            Instructions = Instructions,
            Kernel = agentKernel
        };
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public string Instructions { get; }

    /// <inheritdoc />
    public IReadOnlyList<IVKAtomicTool> Tools { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> Metadata { get; }

    /// <inheritdoc />
    public async Task<VKResult<string>> ExecuteAsync(
        string input,
        VKAgentExecutionContext? context = null,
        VKAgentsArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(input);

        // Apply execution timeout
        TimeSpan timeout = args?.Timeout ?? _options.Timeout ?? TimeSpan.FromSeconds(30);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            var chat = new AgentGroupChat();
            chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

            string finalResponse = string.Empty;

            await foreach (var message in chat.InvokeAsync(InnerAgent, cts.Token).ConfigureAwait(false))
            {
                if (message.Role == AuthorRole.Assistant && !string.IsNullOrWhiteSpace(message.Content))
                {
                    finalResponse = message.Content;
                }
            }

            return VKResult.Success(finalResponse);
        }
        catch (OperationCanceledException)
        {
            return VKResult.Failure<string>(VKAgentErrors.ExecutionFailed);
        }
        catch (Exception)
        {
            return VKResult.Failure<string>(VKAgentErrors.ExecutionFailed); // Map actual error
        }
    }
}
