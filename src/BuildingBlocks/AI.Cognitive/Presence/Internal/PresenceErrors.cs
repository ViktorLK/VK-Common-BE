namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Predefined error codes for the Presence feature.
/// Follows CS.01 and AP.03 internal scope.
/// </summary>
internal static class PresenceErrors
{
    public static class Signal
    {
        public const string Invalid = "Presence.Signal.Invalid";
        public const string OutOfBounds = "Presence.Signal.OutOfBounds";
    }

    public static class SelfMonitor
    {
        public const string EvaluationFailed = "Presence.SelfMonitor.EvaluationFailed";
    }

    public static class Proactive
    {
        public const string CallbackNotRegistered = "Presence.Proactive.CallbackNotRegistered";
        public const string PulseFailed = "Presence.Proactive.PulseFailed";
    }
}
