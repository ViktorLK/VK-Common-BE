namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Request-scoped arguments for configuring Eidos response contract governance per execution turn.
/// Complies with AP.01 (sealed record) and AP.05 (Request-Scoped Args Pattern).
/// </summary>
public sealed record VKAIEidosRequestArgs
{
    /// <summary>
    /// Mutually exclusive contract specification strategy.
    /// </summary>
    public required IVKContractSpec ContractSpec { get; init; }

    /// <summary>
    /// Optional preferred expression mode (StructuredOutput, PromptJson).
    /// If null, negotiated automatically based on model capabilities.
    /// </summary>
    public VKAIEidosExpressionMode? PreferredMode { get; init; }
}
