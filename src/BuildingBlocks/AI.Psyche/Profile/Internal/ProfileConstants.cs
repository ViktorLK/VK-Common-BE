namespace VK.Blocks.AI.Psyche.Profile.Internal;

// [AP.03] Internal constants in internal namespace
internal static class ProfileConstants
{
    public static class XmlTags
    {
        public const string Profile = "profile";
        public const string OutputLanguage = "output_language";
        public const string UserAddressing = "user_addressing";
        public const string InteractionTone = "interaction_tone";
        public const string ResponseVerbosity = "response_verbosity";
        public const string EmojiPolicy = "emoji_policy";
        public const string CustomInstructions = "custom_instructions";
    }

    /// <summary>
    /// Actionable operational directives placed within XML tags for maximum LLM adherence.
    /// </summary>
    public static class Directives
    {
        public static class Addressing
        {
            public const string Prefix = "Address the user as \"";
            public const string Suffix = "\".";
        }

        public static class Tone
        {
            public const string Professional = "Maintain a formal, professional, and objective communication style.";
            public const string Concise = "Maintain a concise, direct, and compact communication style.";
            public const string Friendly = "Maintain an approachable, warm, and conversational tone.";
            public const string Academic = "Maintain a rigorous, academic, and analytical style.";
        }

        public static class Verbosity
        {
            public const string Standard = "Provide standard, well-balanced explanations.";
            public const string Concise = "Provide direct answers. Strictly omit conversational filler, preambles, and pleasantries.";
            public const string Detailed = "Provide comprehensive, detailed explanations with full background context.";
            public const string StepByStep = "Organize output into clear, structured, step-by-step guidance.";
        }

        public static class Emoji
        {
            public const string None = "Strictly do not include any emojis in responses.";
            public const string Minimal = "Use emojis minimally, only as status indicators or list item markers.";
            public const string Rich = "Expressive emojis are welcome throughout responses.";
        }
    }
}
