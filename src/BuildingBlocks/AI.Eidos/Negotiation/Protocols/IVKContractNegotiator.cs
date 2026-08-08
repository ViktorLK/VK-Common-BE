using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Negotiation protocol selecting expression mode (Structured Output / Tool / Prompt JSON) before call.
/// </summary>
public interface IVKContractNegotiator
{
    VKAIEidosNegotiationResult Negotiate(
        VKAIEidosResponseContract contract,
        VKAIEidosProviderCapabilities capabilities);
}
