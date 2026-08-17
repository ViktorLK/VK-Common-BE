using VK.Blocks.AI.Psyche;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Extensions;

/// <summary>
/// Provides strongly-typed access to PWP-specific persona properties stored in the generic Extensions dictionary.
/// </summary>
public static class PwpPersonaExtensions
{
    public const string KeyPersonality = "Personality";
    public const string KeyScenario = "Scenario";
    public const string KeyFirstMessage = "FirstMessage";
    public const string KeyDialogueExamples = "DialogueExamples";

    public static string? GetPersonality(this VKPersonaAnchor anchor)
    {
        return anchor.Extensions.TryGetValue(KeyPersonality, out var obj) ? obj?.ToString() : null;
    }

    public static string? GetScenario(this VKPersonaAnchor anchor)
    {
        return anchor.Extensions.TryGetValue(KeyScenario, out var obj) ? obj?.ToString() : null;
    }

    public static string? GetFirstMessage(this VKPersonaAnchor anchor)
    {
        return anchor.Extensions.TryGetValue(KeyFirstMessage, out var obj) ? obj?.ToString() : null;
    }

    public static string? GetDialogueExamples(this VKPersonaAnchor anchor)
    {
        return anchor.Extensions.TryGetValue(KeyDialogueExamples, out var obj) ? obj?.ToString() : null;
    }
}

public sealed record PwpDialogueExample
{
    public required string UserMessage { get; init; }
    public required string PersonaResponse { get; init; }
}
