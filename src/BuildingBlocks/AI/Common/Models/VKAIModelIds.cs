using System;

namespace VK.Blocks.AI;
/// <summary>
/// Well-known model identifiers per provider.
/// Snapshot: 2026-11-13. Provider lifecycles move fast, so verify against each
/// provider's deprecations page before relying on a legacy entry.
/// </summary>
public static class VKAIModelIds
{
    public static class OpenAI
    {
        public const string Gpt6Astra = "gpt-6-astra";
        public const string Gpt6Sol = "gpt-6-sol";
        public const string Gpt6Luna = "gpt-6-luna";
        public const string Gpt61Sol = "gpt-6.1-sol";

        public static class Embedding
        {
            public const string Small = "text-embedding-3-small";
            public const string Large = "text-embedding-3-large";
            public const string Ada002 = "text-embedding-ada-002";
        }
    }

    public static class Anthropic
    {
        public const string ClaudeFable52 = "claude-fable-5-2";
        public const string ClaudeOpus55 = "claude-opus-5-5";
        public const string ClaudeSonnet55 = "claude-sonnet-5-5";
        public const string ClaudeHaiku55 = "claude-haiku-5-5";
    }

    public static class Google
    {
        public const string Gemini4Argon = "gemini-4-argon";
        public const string Gemini38Flash = "gemini-3.8-flash";
        public const string Gemini36Flash = "gemini-3.6-flash";

        /// <summary>Pro-tier ID as listed by Google; still a preview name.</summary>
        public const string Gemini31ProPreview = "gemini-3.1-pro-preview";

        public const string Gemini38Live = "gemini-3.8-live";
        public const string Gemini38FlashTts = "gemini-3.8-flash-tts";

        public static class Embedding
        {
            public const string TextEmbedding004 = "text-embedding-004";
        }
    }

    public static class DeepSeek
    {
        public const string V4Pro = "deepseek-v4-pro";
        public const string V4Flash = "deepseek-v4-flash";
    }

    public static class XAI
    {
        public const string Grok47 = "grok-4.7";
        public const string Grok46 = "grok-4.6";
        public const string Grok45 = "grok-4.5";
    }

    public static class Mistral
    {
        /// <summary>Public Preview; <c>mistral-large-latest</c> still resolves to Large 3.</summary>
        public const string Large4 = "mistral-large-4";

        public const string Large3 = "mistral-large-2512";
        public const string Medium35 = "mistral-medium-3-5";
        public const string Small4 = "mistral-small-2603";
    }

    /// <summary>Ollama pull tags (<c>family:size</c>).</summary>
    public static class Ollama
    {
        public const string Qwen35_9B = "qwen3.5:9b";
        public const string Qwen35_27B = "qwen3.5:27b";
        public const string Gemma4_12B = "gemma4:12b";
        public const string GptOss20B = "gpt-oss:20b";
        public const string GptOss120B = "gpt-oss:120b";
        public const string Llama33_70B = "llama3.3:70b";
    }

    private readonly record struct ProviderRule(VKAIProviderType Provider, string[] Markers);

    /// <summary>
    /// Ordered, first-match-wins. Specific markers must precede generic ones
    /// (<c>gpt-oss</c> before <c>gpt</c>).
    /// </summary>
    private static readonly ProviderRule[] ProviderRules =
    [
        new(VKAIProviderType.Google, ["gemini"]),
        new(VKAIProviderType.Anthropic, ["claude"]),
        new(VKAIProviderType.XAI, ["grok"]),
        new(VKAIProviderType.DeepSeek, ["deepseek"]),
        new(VKAIProviderType.Mistral, ["mistral", "ministral", "codestral", "devstral"]),
        new(VKAIProviderType.Ollama, ["gpt-oss"]),
        new(VKAIProviderType.OpenAI, ["gpt"]),
        new(VKAIProviderType.Ollama, ["llama", "qwen", "gemma"]),
    ];

    /// <summary>
    /// Resolves the corresponding <see cref="VKAIProviderType"/> based on well-known model naming conventions.
    /// Returns <c>null</c> if the provider cannot be unambiguously inferred.
    /// </summary>
    /// <remarks>
    /// Known limitation: Ollama distills such as <c>deepseek-r1:70b</c> resolve to DeepSeek,
    /// because the name alone cannot tell the host apart.
    /// </remarks>
    public static VKAIProviderType? ResolveProvider(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return null;
        }

        var m = modelId.Trim().ToLowerInvariant();

        foreach (var rule in ProviderRules)
        {
            foreach (var marker in rule.Markers)
            {
                if (m.Contains(marker, StringComparison.Ordinal))
                {
                    return rule.Provider;
                }
            }
        }

        // OpenAI o-series (o1, o3-mini, o4-mini ...): prefix match only, so that
        // an "o1" substring inside an unrelated name cannot misresolve.
        if (m.Length >= 2 && m[0] == 'o' && char.IsAsciiDigit(m[1]))
        {
            return VKAIProviderType.OpenAI;
        }

        if (m.StartsWith("phi", StringComparison.Ordinal))
        {
            return VKAIProviderType.Ollama;
        }

        return null;
    }
}
