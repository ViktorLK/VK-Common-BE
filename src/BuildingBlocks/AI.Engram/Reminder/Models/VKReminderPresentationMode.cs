namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines presentation modes for fired prospective reminders.
/// </summary>
public enum VKReminderPresentationMode
{
    /// <summary>
    /// Inject into Psyche prompt context (Directive tier).
    /// </summary>
    InjectIntoContext,

    /// <summary>
    /// Proactively prompt the user in dialogue.
    /// </summary>
    ProactivePrompt,

    /// <summary>
    /// Make passively available in system state without direct prompt injection.
    /// </summary>
    PassiveAvailable
}
