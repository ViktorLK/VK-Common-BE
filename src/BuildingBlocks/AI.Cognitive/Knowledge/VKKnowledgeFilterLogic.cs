namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the logical combination rules used when matching multiple trigger keys.
/// Follows AP.03.
/// </summary>
public enum VKKnowledgeFilterLogic
{
    /// <summary>
    /// Matches if any of the keyword tokens in the trigger are present.
    /// </summary>
    AndAny = 0,

    /// <summary>
    /// Matches only if all keyword tokens in the trigger are present.
    /// </summary>
    AndAll = 1,

    /// <summary>
    /// Matches only if none of the keyword tokens in the trigger are present.
    /// </summary>
    NotAny = 2,

    /// <summary>
    /// Matches if some tokens are not present.
    /// </summary>
    NotAll = 3
}
