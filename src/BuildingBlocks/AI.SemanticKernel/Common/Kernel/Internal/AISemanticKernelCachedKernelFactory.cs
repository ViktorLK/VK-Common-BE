using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

/// <summary>
/// A high-performance decorator for <see cref="IAISemanticKernelKernelFactory"/> that caches Kernel instances 
/// based on their configuration fingerprint.
/// </summary>
internal sealed class AISemanticKernelCachedKernelFactory(
    IAISemanticKernelKernelFactory innerFactory,
    IMemoryCache cache,
    IVKAISemanticKernelOptionsProvider AISemanticKernelOptionsProvider,
    IOptions<VKAIOptions> globalOptions,
    IVKChatOptionsProvider chatOptionsProvider,
    IOptions<VKEmbeddingsOptions> embeddingOptions) : IAISemanticKernelKernelFactory
{
    private static readonly TimeSpan DefaultAbsoluteExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan DefaultSlidingExpiration = TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public Microsoft.SemanticKernel.Kernel CreateKernel()
    {
        var options = AISemanticKernelOptionsProvider.GetOptions();

        // If caching is disabled, bypass and return a fresh instance
        if (!options.EnableKernelCaching)
        {
            return innerFactory.CreateKernel();
        }

        var fingerprint = GenerateSecureFingerprint();

        return cache.GetOrCreate(fingerprint, entry =>
        {
            // Resource Governance: LRU and Lifespan
            entry.SetAbsoluteExpiration(DefaultAbsoluteExpiration);
            entry.SetSlidingExpiration(DefaultSlidingExpiration);
            entry.SetSize(1); // Each kernel instance counts as 1 unit
            entry.SetPriority(CacheItemPriority.High);

            return innerFactory.CreateKernel();
        })!;
    }

    /// <summary>
    /// Generates a deterministic SHA256 hash based on all configuration dimensions 
    /// that affect the Kernel build process.
    /// </summary>
    private string GenerateSecureFingerprint()
    {
        var AISemanticKernelOptions = AISemanticKernelOptionsProvider.GetOptions();
        var globalAi = globalOptions.Value;
        var chat = chatOptionsProvider.GetOptions();
        var embed = embeddingOptions.Value;

        Span<char> initialBuffer = stackalloc char[256];
        using var sb = new VKValueStringBuilder(initialBuffer);

        // 1. Core AI Strategy & 2. Chat Feature Connectivity & 3. Embedding Feature Connectivity
        sb.Append(globalAi.HttpClientName ?? "default");
        sb.Append('|');
        sb.Append(chat.Provider?.ToString());
        sb.Append('|');
        sb.Append(chat.ModelId);
        sb.Append('|');
        sb.Append(chat.Endpoint);
        sb.Append('|');
        sb.Append(chat.ApiKey?.Reveal() ?? "null");
        sb.Append('|');
        sb.Append(embed.Provider?.ToString());
        sb.Append('|');
        sb.Append(embed.ModelId);
        sb.Append('|');
        sb.Append(embed.Endpoint);
        sb.Append('|');
        sb.Append(embed.ApiKey?.Reveal() ?? "null");
        sb.Append('|');

        // 4. SK Specific Infrastructure
        sb.Append(AISemanticKernelOptions.OrgId);
        sb.Append('|');
        sb.Append(AISemanticKernelOptions.DeploymentName);
        sb.Append('|');
        sb.Append(AISemanticKernelOptions.TemplateFormat);
        sb.Append('|');
        sb.Append(AISemanticKernelOptions.EnableNativePlanners.ToString());
        sb.Append('|');

        // 5. Plugins Feature Set (Deterministic ordering)
        sb.Append(AISemanticKernelOptions.Plugins.AutoDiscoveryEnabled.ToString());
        sb.Append('|');

        if (AISemanticKernelOptions.Plugins.Types.Count > 0)
        {
            var sortedTypes = AISemanticKernelOptions.Plugins.Types.OrderBy(x => x.Key);
            foreach (var kvp in sortedTypes)
            {
                sb.Append(kvp.Key);
                sb.Append(':');
                sb.Append(kvp.Value);
                sb.Append(';');
            }
        }

        if (AISemanticKernelOptions.Plugins.AssembliesToScan.Count > 0)
        {
            var sortedAssemblies = AISemanticKernelOptions.Plugins.AssembliesToScan.OrderBy(x => x);
            foreach (var asm in sortedAssemblies)
            {
                sb.Append(asm);
                sb.Append(';');
            }
        }

        // 6. Secure Hashing (SHA256)
        byte[] inputBytes = Encoding.UTF8.GetBytes(sb.ToString());
        byte[] hashBytes = SHA256.HashData(inputBytes);

        return "AISemanticKernel|K|" + Convert.ToHexString(hashBytes);
    }
}
