namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines standard prompt insertion positions compatible with SillyTavern World Info.
/// </summary>
public enum VKKnowledgePositions
{
    /// <summary>
    /// Inserts the entry before character/user definitions (Before Definitions).
    /// </summary>
    BeforeDefs,

    /// <summary>
    /// Inserts the entry after character/user definitions (After Definitions).
    /// </summary>
    AfterDefs,

    /// <summary>
    /// Inserts the entry before example messages (Before Example Messages).
    /// </summary>
    BeforeExampleMessages,

    /// <summary>
    /// Inserts the entry after example messages (After Example Messages).
    /// </summary>
    AfterExampleMessages,

    /// <summary>
    /// Inserts the entry before the Author's Note.
    /// </summary>
    BeforeAuthorNote,

    /// <summary>
    /// Inserts the entry after the Author's Note.
    /// </summary>
    AfterAuthorNote,

    /// <summary>
    /// Inserts the entry into a System message at a specific history depth (System + Depth).
    /// </summary>
    SystemAtDepth,

    /// <summary>
    /// Inserts the entry into a User message at a specific history depth (User + Depth).
    /// </summary>
    UserAtDepth,

    /// <summary>
    /// Inserts the entry into an Assistant message at a specific history depth (Assistant + Depth).
    /// </summary>
    AssistantAtDepth
}
