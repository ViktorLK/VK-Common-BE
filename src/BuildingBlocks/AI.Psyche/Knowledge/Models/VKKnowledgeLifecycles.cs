// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Predefined lifecycle constants (T-Shirt Sizing) for knowledge entries.
/// Helps authors quickly configure the `StickyTurns` property without guessing numbers.
/// </summary>
public static class VKKnowledgeLifecycles
{
    /// <summary>
    /// Presets for the `StickyTurns` property.
    /// Controls how long the knowledge remains active after being triggered.
    /// </summary>
    public static class Sticky
    {
        /// <summary>
        /// Flash (0): Active only for the exact turn it is triggered.
        /// Best for transient UI elements, immediate action reactions, or passing name drops.
        /// </summary>
        public const int Flash = 0;

        /// <summary>
        /// Short (2): Short-term memory.
        /// Best for actions that require a couple of turns to fully resolve (e.g. pulling a weapon, casting a spell).
        /// </summary>
        public const int Short = 2;

        /// <summary>
        /// Topic (5): Standard working memory.
        /// Best for active conversation topics, current room descriptions, or temporary emotional states.
        /// </summary>
        public const int Topic = 5;

        /// <summary>
        /// Scene (15): Long-term working memory.
        /// Best for entire combat encounters, long interrogations, or complex puzzles that span many turns.
        /// </summary>
        public const int Scene = 15;

        /// <summary>
        /// Anchor (-1): Infinite. Effectively permanent for the duration of a scene after triggering.
        /// Best for overarching scene rules, fixed locations, or major plot states.
        /// </summary>
        public const int Anchor = -1;
    }

    /// <summary>
    /// Presets for the `CooldownTurns` property.
    /// Controls how long the engine ignores subsequent keyword matches after a successful trigger.
    /// </summary>
    public static class Cooldown
    {
        /// <summary>
        /// None (0): Can be triggered every single turn.
        /// </summary>
        public const int None = 0;

        /// <summary>
        /// Short (3): Prevents spamming in back-to-back turns.
        /// </summary>
        public const int Short = 3;

        /// <summary>
        /// Rhythm (5): Paces out conversational repetition. 
        /// Best for character catchphrases or recurring jokes.
        /// </summary>
        public const int Rhythm = 5;

        /// <summary>
        /// Long (10): Prevents the knowledge from re-triggering for a significant portion of the conversation.
        /// </summary>
        public const int Long = 10;

        /// <summary>
        /// Once (-1): Infinite cooldown. Effectively triggers only once per scene/session.
        /// </summary>
        public const int Once = -1;
    }

    /// <summary>
    /// Presets for the `DelayTurns` property.
    /// Controls the latency before a triggered knowledge becomes active in the prompt.
    /// </summary>
    public static class Delay
    {
        /// <summary>
        /// Immediate (0): Injected into the prompt on the exact turn it was triggered.
        /// </summary>
        public const int Immediate = 0;

        /// <summary>
        /// NextTurn (1): Injected on the next turn. Useful for delayed reactions or "fuse" mechanics.
        /// </summary>
        public const int NextTurn = 1;

        /// <summary>
        /// Slow (3): Injected several turns later. Useful for simmering plots or slow realizations.
        /// </summary>
        public const int Slow = 3;
    }
}
