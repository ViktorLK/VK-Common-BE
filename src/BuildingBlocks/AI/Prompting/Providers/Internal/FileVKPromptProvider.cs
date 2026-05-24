using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Prompting.Providers.Internal;

/// <summary>
/// Loads prompt templates from a local file system directory.
/// </summary>
internal sealed class FileVKPromptProvider : IVKPromptProvider
{
    private readonly string _baseDirectory;

    public FileVKPromptProvider(string baseDirectory)
    {
        _baseDirectory = VKGuard.NotNullOrWhiteSpace(baseDirectory);
    }

    public async Task<VKResult<VKPromptTemplate>> GetPromptAsync(string promptId, string? version = null, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(promptId);

        string fileName = version is not null ? $"{promptId}_v{version}.txt" : $"{promptId}.txt";
        string filePath = Path.Combine(_baseDirectory, fileName);

        if (!File.Exists(filePath))
        {
            return VKResult.Failure<VKPromptTemplate>(VKError.NotFound("Prompt.NotFound", $"File prompt '{fileName}' not found at '{_baseDirectory}'."));
        }

        try
        {
            // Note: In .NET 8+, File.ReadAllTextAsync can take cancellationToken
            string content = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);

            var template = new VKPromptTemplate
            {
                Id = promptId,
                Version = version ?? "1.0",
                Text = content,
                Role = VKChatRole.System
            };

            return VKResult.Success(template);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<VKPromptTemplate>(VKError.Failure("Prompt.ReadError", $"Failed to read prompt file: {ex.Message}"));
        }
    }
}
