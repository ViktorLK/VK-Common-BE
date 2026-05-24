using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Cognitive.Reasoning.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Reasoning.Internal;

internal sealed class DefaultReasoningPlanner : IVKReasoningPlanner
{
    private readonly IVKChatEngine _chatEngine;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly ILogger<DefaultReasoningPlanner> _logger;

    public DefaultReasoningPlanner(
        IVKChatEngine chatEngine,
        IVKGuidGenerator guidGenerator,
        IVKJsonSerializer jsonSerializer,
        ILogger<DefaultReasoningPlanner> logger)
    {
        _chatEngine = VKGuard.NotNull(chatEngine);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKGoal>> PlanAsync(
        string goal,
        VKReasoningArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(goal);

        try
        {
            return await DecomposeGoalAsync(goal, args, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ReasoningDiagnostics.GoalDecompositionFailed(_logger, ex, goal);
            return VKResult.Failure<VKGoal>(VKCognitiveErrors.OperationFailed);
        }
    }

    private sealed record DecompositionStep
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public string? Target { get; init; }
    }

    private static string CleanMarkdownJson(string rawJson)
    {
        string cleaned = rawJson.Trim();
        if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned.Substring(7);
        }
        else if (cleaned.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned.Substring(3);
        }

        if (cleaned.EndsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 3);
        }

        return cleaned.Trim();
    }

    private async Task<VKResult<VKGoal>> DecomposeGoalAsync(string goal, VKReasoningArgs? args, CancellationToken ct)
    {
        var decompositionPrompt =
            $"""
            Analyze the following goal and decompose it into a sequence of logical steps.
            For each step, specify:
            1. A concise Name.
            2. A detailed Description.
            3. A Target (the tool or expert required, if any).
            
            Goal: {goal}
            
            Output as a JSON array of objects with 'Name', 'Description', and 'Target' properties.
            """;

        var messages = new[] { VKChatMessage.FromText(VKChatRole.User, decompositionPrompt) };
        var result = await _chatEngine.SendAsync(messages, null, ct).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            return VKResult.Failure<VKGoal>(result.FirstError);
        }

        string? json = result.Value.Message.Content;
        if (string.IsNullOrWhiteSpace(json))
        {
            return VKResult.Failure<VKGoal>(VKCognitiveErrors.OperationFailed);
        }

        try
        {
            string cleanedJson = CleanMarkdownJson(json);
            var parsedSteps = _jsonSerializer.Deserialize<List<DecompositionStep>>(cleanedJson);

            if (parsedSteps == null || parsedSteps.Count == 0)
            {
                return VKResult.Failure<VKGoal>(VKCognitiveErrors.OperationFailed);
            }

            var steps = new List<VKStep>();
            foreach (var step in parsedSteps)
            {
                steps.Add(new VKStep
                {
                    Id = _guidGenerator.Create().ToString(),
                    Name = step.Name,
                    Description = step.Description,
                    Target = step.Target
                });
            }

            // Closed-loop Self-Reflection Step
            try
            {
                var serializedSteps = _jsonSerializer.Serialize(parsedSteps);
                var reflectionPrompt =
                    $"""
                    Review the following goal and proposed decomposed steps.
                    Determine if this plan is logical, safe, and complete.
                    If there is a missing step or safety gap, output a single corrective step.
                    If the plan is perfect, output 'PERFECT'.

                    Goal: {goal}
                    Steps: {serializedSteps}

                    Output in JSON format with properties 'Status' (either 'PERFECT' or 'CORRECTION') and 'Description' (containing the corrective step details).
                    """;

                var reflectMessages = new[] { VKChatMessage.FromText(VKChatRole.User, reflectionPrompt) };
                var reflectResult = await _chatEngine.SendAsync(reflectMessages, null, ct).ConfigureAwait(false);

                if (reflectResult.IsSuccess && !string.IsNullOrWhiteSpace(reflectResult.Value.Message.Content))
                {
                    var cleanedReflect = CleanMarkdownJson(reflectResult.Value.Message.Content);
                    var reflection = _jsonSerializer.Deserialize<VKReflectionResult>(cleanedReflect);
                    if (reflection != null && string.Equals(reflection.Status, "CORRECTION", StringComparison.OrdinalIgnoreCase))
                    {
                        steps.Add(new VKStep
                        {
                            Id = _guidGenerator.Create().ToString(),
                            Name = "Self-Correction & Refinement",
                            Description = reflection.Description ?? "Execute additional safety checks and self-correction overrides.",
                            Target = "SystemSelfMonitor"
                        });
                    }
                }
            }
            catch
            {
                // Soft-fail: if reflection prompt fails, keep original steps (resiliency)
            }

            return VKResult.Success(new VKGoal
            {
                Id = _guidGenerator.Create().ToString(),
                Title = $"Plan: {(goal.Length > 30 ? goal.Substring(0, 30) + "..." : goal)}",
                Description = goal,
                Steps = steps
            });
        }
        catch (Exception ex)
        {
            ReasoningDiagnostics.StepParsingFailed(_logger, ex);
            return VKResult.Failure<VKGoal>(VKCognitiveErrors.OperationFailed);
        }
    }

    private sealed record VKReflectionResult
    {
        public required string Status { get; init; }
        public string? Description { get; init; }
    }
}
