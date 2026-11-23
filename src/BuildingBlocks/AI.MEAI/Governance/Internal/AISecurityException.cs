using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Governance.Internal;

/// <summary>
/// Internal exception thrown by guardrail clients to communicate security or policy violations.
/// Mapped to typed VKError at adapter boundary (CS.01).
/// </summary>
internal sealed class AISecurityException(VKError error, float confidenceScore = 1.0f) : Exception(error.Description)
{
    public VKError Error { get; } = VKGuard.NotNull(error);
    public float ConfidenceScore { get; } = confidenceScore;
}

