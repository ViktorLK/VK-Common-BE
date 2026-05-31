namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines the logical matching strategy used when evaluating trigger keys.
/// Follows AP.03.
/// </summary>
public enum VKKnowledgeMatchType
{
    /// <summary>
    /// Performs a simple substring containment check.
    /// </summary>
    Contains = 0,

    /// <summary>
    /// Performs a strict whole-word match using regex boundary markers (\b).
    /// </summary>
    WholeWord = 1,

    /// <summary>
    /// Performs a regular expression match.
    /// </summary>
    Regex = 2
}
