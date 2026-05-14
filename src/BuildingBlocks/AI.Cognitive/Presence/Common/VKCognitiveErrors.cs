using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Standard error constants for the AI Cognitive building block.
/// </summary>
public static class VKCognitiveErrors
{
    /// <summary>
    /// Errors related to general cognitive operations.
    /// </summary>
    public static class Internal
    {
        /// <summary>
        /// Error returned when a cognitive operation fails unexpectedly.
        /// </summary>
        public static readonly VKError OperationFailed = new("AI.Cognitive.Internal.OperationFailed", "An unexpected error occurred during the cognitive operation.");
    }
}
