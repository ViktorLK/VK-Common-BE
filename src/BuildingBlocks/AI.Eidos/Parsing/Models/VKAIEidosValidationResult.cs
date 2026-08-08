using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public sealed record VKAIEidosValidationResult
{
    public bool IsValid { get; init; } = true;
    public IReadOnlyList<string> MissingProperties { get; init; } = [];
    public IReadOnlyList<string> ErrorMessages { get; init; } = [];
}
