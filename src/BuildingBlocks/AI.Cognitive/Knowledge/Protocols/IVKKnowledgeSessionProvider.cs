namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Provides access to the active chat session identifier for tracking runtime turn timers.
/// </summary>
public interface IVKKnowledgeSessionProvider
{
    /// <summary>
    /// Gets the current active session identifier.
    /// </summary>
    string GetCurrentSessionId();
}
