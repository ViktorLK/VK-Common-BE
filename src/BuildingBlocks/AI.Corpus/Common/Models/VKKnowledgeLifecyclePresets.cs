namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Predefined lifecycle constants (T-Shirt Sizing) for knowledge entries in AI.Corpus.
/// </summary>
public static class VKKnowledgeLifecyclePresets
{
    /// <summary>
    /// Presets for the `StickyTurns` property.
    /// Controls how long the knowledge remains active after being triggered.
    /// </summary>
    public static class Sticky
    {
        /// <summary>
        /// Flash (0): Active only for the exact turn it is triggered.
        /// </summary>
        public const int Flash = 0;

        /// <summary>
        /// Short (2): Short-term memory.
        /// </summary>
        public const int Short = 2;

        /// <summary>
        /// Topic (5): Standard working memory.
        /// </summary>
        public const int Topic = 5;

        /// <summary>
        /// Scene (15): Long-term working memory.
        /// </summary>
        public const int Scene = 15;

        /// <summary>
        /// Anchor (-1): Infinite.
        /// </summary>
        public const int Anchor = -1;
    }

    /// <summary>
    /// Presets for the `CooldownTurns` property.
    /// Controls how long the engine ignores subsequent matches after a successful trigger.
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
        /// </summary>
        public const int Rhythm = 5;

        /// <summary>
        /// Long (10): Prevents re-triggering for a significant portion of the conversation.
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
        /// NextTurn (1): Injected on the next turn.
        /// </summary>
        public const int NextTurn = 1;

        /// <summary>
        /// Slow (3): Injected several turns later.
        /// </summary>
        public const int Slow = 3;
    }
}
