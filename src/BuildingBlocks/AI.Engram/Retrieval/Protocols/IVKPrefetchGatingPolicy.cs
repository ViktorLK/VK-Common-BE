namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy contract for evaluating whether a user input should trigger LLM intent Cue extraction.
/// </summary>
public interface IVKPrefetchGatingPolicy
{
    /// <summary>
    /// Evaluates whether the given user input should trigger intent Cue extraction.
    /// </summary>
    /// <param name="input">The raw user input text.</param>
    /// <returns>True if intent Cue extraction should be triggered; otherwise false.</returns>
    bool ShouldTriggerIntentExtraction(string input);
}
