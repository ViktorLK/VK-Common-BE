using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines a contract for parsing and rendering prompt templates.
/// </summary>
public interface IVKPromptTemplateEngine
{
    /// <summary>
    /// Renders a prompt template string by substituting variables with provided values.
    /// </summary>
    /// <param name="templateText">The template string containing placeholders.</param>
    /// <param name="variables">A dictionary of variables to inject into the template.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The fully rendered prompt string.</returns>
    Task<VKResult<string>> RenderAsync(
        string templateText,
        IDictionary<string, object?>? variables = null,
        CancellationToken cancellationToken = default);
}
