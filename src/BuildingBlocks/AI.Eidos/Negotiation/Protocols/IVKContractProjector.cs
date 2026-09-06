using System;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Projects a neutral response contract into mode-specific provider representations (JSON schema or System Prompt).
/// </summary>
public interface IVKContractProjector
{
    string ProjectToIntermediateRepresentation(
        VKAIEidosResponseContract contract,
        VKAIEidosExpressionMode mode);

    VKChatArgs ApplyProjection(
        VKChatArgs chatArgs,
        VKAIEidosResponseContract contract,
        VKAIEidosExpressionMode mode);

    VKPromptFragment GetHeaderProtocolFragment();

    VKPromptFragment GetTailSchemaFragment(
        VKAIEidosResponseContract contract);
}
