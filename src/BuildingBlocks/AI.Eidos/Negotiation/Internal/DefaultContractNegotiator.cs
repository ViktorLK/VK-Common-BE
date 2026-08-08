using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

internal sealed class DefaultContractNegotiator(
    IOptions<VKNegotiationOptions>? options = null) : IVKContractNegotiator
{
    private readonly VKNegotiationOptions _options = options?.Value ?? new VKNegotiationOptions();

    public VKAIEidosNegotiationResult Negotiate(
        VKAIEidosResponseContract contract,
        VKAIEidosProviderCapabilities capabilities)
    {
        VKGuard.NotNull(contract);
        VKGuard.NotNull(capabilities);

        if (capabilities.SupportsNativeStructuredOutput && capabilities.SupportsToolCalling)
        {
            return new VKAIEidosNegotiationResult
            {
                SelectedMode = _options.DefaultPreferredMode,
                Contract = contract
            };
        }

        if (capabilities.SupportsNativeStructuredOutput)
        {
            return new VKAIEidosNegotiationResult
            {
                SelectedMode = VKAIEidosExpressionMode.StructuredOutput,
                Contract = contract
            };
        }

        if (capabilities.SupportsToolCalling)
        {
            return new VKAIEidosNegotiationResult
            {
                SelectedMode = VKAIEidosExpressionMode.ToolCall,
                Contract = contract
            };
        }

        return new VKAIEidosNegotiationResult
        {
            SelectedMode = VKAIEidosExpressionMode.PromptJson,
            Contract = contract,
            SystemPromptInstruction = $"Respond strictly using JSON matching schema: {contract.Schema.RawJsonSchema}"
        };
    }
}
