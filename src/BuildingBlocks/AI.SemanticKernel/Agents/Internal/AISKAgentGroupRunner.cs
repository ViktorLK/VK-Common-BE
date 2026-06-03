using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Agents.Internal;

/// <summary>
/// Semantic Kernel implementation of <see cref="IVKAgentGroup"/> utilizing native SK multi-agent coordination.
/// </summary>
internal sealed class AISKAgentGroupRunner : IVKAgentGroup
{
    private readonly Microsoft.SemanticKernel.Kernel _kernel;

    public AISKAgentGroupRunner(Microsoft.SemanticKernel.Kernel kernel)
    {
        _kernel = VKGuard.NotNull(kernel); // [AP.01]
    }

    /// <inheritdoc />
    public async Task<VKResult<VKAgentGroupResult>> ExecuteAsync(
        string input,
        IReadOnlyList<IVKAgent> agents,
        VKAgentGroupOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(input); // [AP.01]
        VKGuard.NotEmpty(agents); // [AP.01]

        var opt = options ?? new VKAgentGroupOptions();

        try
        {
            // 1. Resolve SK Agents from IVKAgent (ensure they are AISKAgent instances)
            var skAgents = new List<ChatCompletionAgent>();
            foreach (var agent in agents)
            {
                if (agent is AISKAgent aiskAgent)
                {
                    skAgents.Add(aiskAgent.InnerAgent);
                }
                else
                {
                    return VKResult.Failure<VKAgentGroupResult>(
                        new VKError("AI.Agents.UnsupportedAgentType",
                            $"Agent '{agent.Name}' is not a Semantic Kernel agent."));
                }
            }

            // 2. Setup Selection Strategy
            SelectionStrategy selectionStrategy = opt.SelectionMode switch
            {
                VKAgentSelectionMode.LLMBased => new KernelFunctionSelectionStrategy(
                    CreateSelectionFunction(skAgents),
                    _kernel)
                {
                    ResultParser = (result) => result.GetValue<string>() ?? skAgents[0].Name!
                },
                _ => new SequentialSelectionStrategy()
            };

            // 3. Setup Termination Strategy
            var terminationStrategy = new KeywordTerminationStrategy(opt.TerminationKeywords)
            {
                AutomaticReset = true,
                MaximumIterations = opt.MaxRounds
            };

            // 4. Create Group Chat
            var chat = new AgentGroupChat(skAgents.ToArray())
            {
                ExecutionSettings = new AgentGroupChatSettings
                {
                    SelectionStrategy = selectionStrategy,
                    TerminationStrategy = terminationStrategy
                }
            };

            // 5. Add Initial Prompt
            chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

            // 6. Invoke and Stream Results
            var historyMessages = new List<VKChatMessage>();
            string finalOutput = string.Empty;
            int rounds = 0;

            await foreach (var message in chat.InvokeAsync(cancellationToken).ConfigureAwait(false)) // [CS.03]
            {
                rounds++;
                if (message.Content is not null)
                {
                    finalOutput = message.Content;
                    historyMessages.Add(new VKChatMessage
                    {
                        Role = message.Role == AuthorRole.User ? VKChatRole.User : VKChatRole.Assistant,
                        Content = message.Content,
                        AuthorName = message.AuthorName
                    });
                }
            }

            return VKResult.Success(new VKAgentGroupResult
            {
                Output = finalOutput,
                Messages = historyMessages,
                CompletedRounds = rounds
            });
        }
        catch (Exception ex)
        {
            return VKResult.Failure<VKAgentGroupResult>(
                new VKError("AI.Agents.GroupChatFailed",
                    $"Cooperative execution failed: {ex.Message}"));
        }
    }

    private KernelFunction CreateSelectionFunction(IReadOnlyList<ChatCompletionAgent> agents)
    {
        var agentDescriptions = string.Join("\n", agents.Select(a => $"- {a.Name}: {a.Description}"));
        var agentNames = string.Join(", ", agents.Select(a => a.Name));

        var prompt = @$"
You are a coordinator managing a group of AI agents to solve a user's request.
Based on the conversation history, select the next best agent to speak.

Available Agents:
{agentDescriptions}

Respond ONLY with the name of the selected agent. Do not provide any explanation.
Name must be one of: {agentNames}";

        return _kernel.CreateFunctionFromPrompt(prompt);
    }

    private sealed class KeywordTerminationStrategy(IReadOnlyList<string> keywords) : TerminationStrategy
    {
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        {
            var lastMessage = history.LastOrDefault()?.Content;
            if (string.IsNullOrWhiteSpace(lastMessage)) return Task.FromResult(false);

            foreach (var keyword in keywords)
            {
                if (lastMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }
    }
}
