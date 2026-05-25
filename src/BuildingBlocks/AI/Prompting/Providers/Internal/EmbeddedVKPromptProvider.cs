using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Prompting.Providers.Internal;

/// <summary>
/// Loads prompt templates embedded directly within an assembly.
/// </summary>
internal sealed class EmbeddedVKPromptProvider : IVKPromptProvider
{
    private readonly Assembly _assembly;
    private readonly string _baseNamespace;

    public EmbeddedVKPromptProvider(Assembly assembly, string baseNamespace)
    {
        _assembly = VKGuard.NotNull(assembly);
        _baseNamespace = VKGuard.NotNullOrWhiteSpace(baseNamespace);
    }

    public async Task<VKResult<VKPromptTemplate>> GetPromptAsync(string promptId, string? version = null, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(promptId);

        string resourceName = $"{_baseNamespace}.{promptId}.txt";

        using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return VKResult.Failure<VKPromptTemplate>(VKError.NotFound("Prompt.NotFound", $"Embedded prompt '{promptId}' not found."));
        }

        using var reader = new StreamReader(stream);
        string content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        var template = new VKPromptTemplate
        {
            Id = promptId,
            Version = version ?? "1.0",
            Text = content,
            Role = VKChatRole.System
        };

        return VKResult.Success(template);
    }
}
