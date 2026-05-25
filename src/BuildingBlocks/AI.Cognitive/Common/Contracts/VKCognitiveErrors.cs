using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Standard error constants for the AI Cognitive building block.
/// </summary>
public static class VKCognitiveErrors
{
    public static readonly VKError OperationFailed = VKError.Failure(
        "AI.Cognitive.Internal.OperationFailed",
        "An unexpected error occurred during the cognitive operation.");

    public static readonly VKError PipelineFault = VKError.Failure(
        "Cognitive.PipelineFault",
        "An unexpected error occurred during cognitive pipeline orchestration.");

    public static readonly VKError MemoryAccessFailed = VKError.Failure(
        "Cognitive.MemoryAccessFailed",
        "Failed to access or retrieve memory/knowledge echoes.");

    public static readonly VKError IntentRoutingTimeout = VKError.Failure(
        "Cognitive.IntentRoutingTimeout",
        "The intent routing module timed out or failed to determine the user intent.");

    public static readonly VKError InferenceTimeout = VKError.Failure(
        "Cognitive.InferenceTimeout",
        "The underlying LLM inference stream timed out.");

    public static readonly VKError GovernanceRejected = VKError.Failure(
        "Cognitive.GovernanceRejected",
        "The request was rejected by early governance interceptors.");
}
