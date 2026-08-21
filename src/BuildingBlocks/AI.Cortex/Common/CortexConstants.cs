using System;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Architectural and operational constants for AI.Cortex building block.
/// Follows [BB.04] and [AP.01].
/// </summary>
public static class CortexConstants
{
    public static class Resilience
    {
        public const string DefaultLlmCircuitBreakerKey = "llm-provider";
        public const string DefaultFastToolCircuitBreakerKey = "fast-tool-provider";

        public static readonly TimeSpan DefaultChatTimeout = TimeSpan.FromSeconds(30);
        public const int DefaultChatMaxRetries = 3;
        public const int DefaultChatInitialDelayMs = 500;
        public const double DefaultChatBackoffMultiplier = 2.0;

        public static readonly TimeSpan DefaultFastToolTimeout = TimeSpan.FromSeconds(10);
        public const int DefaultFastToolMaxRetries = 1;
        public const int DefaultFastToolInitialDelayMs = 200;
    }

    public static class Session
    {
        public static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(30);
        public const bool DefaultEnableCrossDayBoundary = true;
    }
}
