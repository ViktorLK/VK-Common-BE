using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Standard error constants for the Psyche Pipeline.
/// </summary>
public static class VKBehaviorsErrors
{
    public static readonly VKError EmptyTapestry = new("AI.Psyche.Pipeline.EmptyTapestry", "Tapestry was not assembled by the pipeline stages.");
    public static readonly VKError EmptyResponse = new("AI.Psyche.Pipeline.EmptyResponse", "The AI pipeline executed but returned an empty response.");
    public static readonly VKError ChatEngineNotFound = new("AI.Psyche.Pipeline.ChatEngineNotFound", "IVKChatEngine is not registered in the service provider.");
    public static readonly VKError Aborted = new("AI.Psyche.Pipeline.Aborted", "The pipeline execution was aborted.");
}
