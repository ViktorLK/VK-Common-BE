namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines the overall conversational tone and interaction posture.
/// Follows AP.01 and AP.03.
/// </summary>
public enum VKInteractionTone : byte
{
    /// <summary>
    /// Formal, professional, objective, and neutral communication style.
    /// </summary>
    Professional = 0,

    /// <summary>
    /// Direct, compact, and to-the-point without superfluous filler.
    /// </summary>
    Concise = 1,

    /// <summary>
    /// Approachable, warm, friendly, and natural conversational tone.
    /// </summary>
    Friendly = 2,

    /// <summary>
    /// In-depth, analytical, rigorous, and academic communication style.
    /// </summary>
    Academic = 3
}
