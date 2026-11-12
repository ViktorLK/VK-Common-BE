using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI;

/// <summary>
/// Diagnostic EventId offsets for the AI building block features.
/// Follows BB.04: Library-level sub-range allocation based on <see cref="VKDiagnosticOffsets.AI"/>.
/// </summary>
public static class VKAIDiagnosticOffsets
{
    private const int Base = VKDiagnosticOffsets.AI;

    public const int Agents = Base + 1000;
    public const int AtomicTools = Base + 2000;
    public const int Audio = Base + 3000;
    public const int Chat = Base + 4000;
    public const int Guardrails = Base + 5000;
    public const int Prompting = Base + 6000;
    public const int Text = Base + 7000;
    public const int Tokenics = Base + 8000;
    public const int Vectorics = Base + 9000;
}

