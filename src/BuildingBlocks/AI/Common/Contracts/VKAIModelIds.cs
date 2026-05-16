namespace VK.Blocks.AI;

public static class VKAIModelIds
{
    public static class OpenAI
    {
        public const string Gpt4O = "gpt-4o";
        public const string Gpt4OMini = "gpt-4o-mini";
        public const string Gpt4Turbo = "gpt-4-turbo";
        public const string O1 = "o1";
        public const string O1Mini = "o1-mini";
        public const string O1Preview = "o1-preview";
        public const string O3Mini = "o3-mini";

        public static class Embedding
        {
            public const string Small = "text-embedding-3-small";
            public const string Large = "text-embedding-3-large";
            public const string Ada002 = "text-embedding-ada-002";
        }
    }

    public static class Anthropic
    {
        public const string Claude35Sonnet = "claude-3-5-sonnet-latest";
        public const string Claude35Haiku = "claude-3-5-haiku-latest";
        public const string Claude3Opus = "claude-3-opus-latest";
        public const string Claude3Sonnet = "claude-3-sonnet-20240229";
        public const string Claude3Haiku = "claude-3-haiku-20240307";
    }

    public static class Google
    {
        public const string Gemini20Flash = "gemini-2.0-flash";
        public const string Gemini20FlashLite = "gemini-2.0-flash-lite-preview-02-05";
        public const string Gemini20Pro = "gemini-2.0-pro-exp-02-05";
        public const string Gemini20FlashThinking = "gemini-2.0-flash-thinking-exp-01-21";
        public const string Gemini15Pro = "gemini-1.5-pro";
        public const string Gemini15ProLatest = "gemini-1.5-pro-latest";
        public const string Gemini15Flash = "gemini-1.5-flash";
        public const string Gemini15FlashLatest = "gemini-1.5-flash-latest";
        public const string Gemini15Flash8B = "gemini-1.5-flash-8b";
        public const string Gemini15Flash8BLatest = "gemini-1.5-flash-8b-latest";
        public const string Gemini10Pro = "gemini-1.0-pro";

        public static class Embedding
        {
            public const string TextEmbedding004 = "text-embedding-004";
        }
    }
}
