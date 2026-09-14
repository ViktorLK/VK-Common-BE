using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Prompting.Internal;

/// <summary>
/// A zero-dependency minimalist template engine that simply replaces {{Key}} with variable values.
/// </summary>
internal sealed class BasicVKPromptTemplateEngine : IVKPromptTemplateEngine
{
    public Task<VKResult<string>> RenderAsync(
        string templateText,
        IDictionary<string, object?>? variables = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(templateText);

        if (variables is null || variables.Count == 0 || !templateText.Contains("{{"))
        {
            return Task.FromResult(VKResult.Success(templateText));
        }

        var sb = new StringBuilder(templateText);
        foreach (var kvp in variables)
        {
            if (kvp.Value is null)
            {
                continue;
            }

            string placeholder = "{{" + kvp.Key + "}}";
            sb.Replace(placeholder, kvp.Value.ToString()!);
        }

        return Task.FromResult(VKResult.Success(sb.ToString()));
    }
}
