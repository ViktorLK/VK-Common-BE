using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Guardrails.Internal;

/// <summary>
/// Predefined error constants for the Afferent Guardrails slice.
/// Follows CS.01.
/// </summary>
internal static class GuardrailsErrors
{
    public static readonly VKError PrivacyViolation = VKError.Failure(
        "Afferent.Guardrails.PrivacyViolation",
        "A privacy violation occurred during input processing.");

    public static readonly VKError InjectionDetected = VKError.Failure(
        "Afferent.Guardrails.InjectionDetected",
        "A prompt injection attempt was detected.");

    public static readonly VKError ContentFlagged = VKError.Failure(
        "Afferent.Guardrails.ContentFlagged",
        "The content was flagged by moderation policies.");
}
