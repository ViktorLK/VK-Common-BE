using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Standard error constants for the Psyche Pipeline.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static error definitions and constant descriptors.")]
public static class VKPipelineErrors
{
    public static readonly VKError EmptyTapestry = new("AI.Psyche.Pipeline.EmptyTapestry", "Tapestry was not assembled by the pipeline stages.");
    public static readonly VKError EmptyResponse = new("AI.Psyche.Pipeline.EmptyResponse", "The AI pipeline executed but returned an empty response.");
    public static readonly VKError ChatEngineNotFound = new("AI.Psyche.Pipeline.ChatEngineNotFound", "IVKChatEngine is not registered in the service provider.");
    public static readonly VKError ProviderNotConfigured = new("AI.Psyche.Pipeline.ProviderNotConfigured", "AI provider is not configured in request arguments, ambient options, or model catalog.");
    public static readonly VKError ModelNotConfigured = new("AI.Psyche.Pipeline.ModelNotConfigured", "AI model is not configured in request arguments or ambient options.");
    public static readonly VKError Aborted = new("AI.Psyche.Pipeline.Aborted", "The pipeline execution was aborted.");
}
