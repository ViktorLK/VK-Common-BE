using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Defines the safety validation contract for Guardrails.
/// Complies with CS.01, CS.03, and AP.01.
/// </summary>
public interface IVKGuardrail
{
    /// <summary>
    /// Validates the safety of the provided input text.
    /// </summary>
    /// <param name="text">The input text to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating safety compliance. Can contain sanitized text.</returns>
    Task<VKResult<string>> ValidateSafetyAsync(string text, CancellationToken cancellationToken = default);
}
