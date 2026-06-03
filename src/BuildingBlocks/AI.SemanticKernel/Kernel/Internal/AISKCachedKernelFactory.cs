using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

/// <summary>
/// A high-performance decorator for <see cref="IAISKKernelFactory"/> that caches Kernel instances 
/// based on their configuration fingerprint.
/// </summary>
internal sealed class AISKCachedKernelFactory(
    IAISKKernelFactory innerFactory,
    IMemoryCache cache,
    IVKAISKOptionsProvider aiskOptionsProvider,
    IOptions<VKAIDefaultsOptions> globalOptions,
    IVKChatOptionsProvider chatOptionsProvider,
    IOptions<VKEmbeddingsOptions> embeddingOptions) : IAISKKernelFactory
{
    private static readonly TimeSpan DefaultAbsoluteExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan DefaultSlidingExpiration = TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public Microsoft.SemanticKernel.Kernel CreateKernel()
    {
        var options = aiskOptionsProvider.GetOptions();

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
        var aiskOptions = aiskOptionsProvider.GetOptions();
        var globalAi = globalOptions.Value;
        var chat = chatOptionsProvider.GetOptions();
        var embed = embeddingOptions.Value;

        var sb = new StringBuilder();

        // 1. Core AI Strategy
        sb.Append(globalAi.Provider).Append('|');

        // 2. Chat Feature Connectivity
        sb.Append(chat.Provider).Append('|')
          .Append(chat.ModelId).Append('|')
          .Append(chat.Endpoint).Append('|')
          .Append(chat.ApiKey?.Reveal() ?? "null").Append('|');

        // 3. Embedding Feature Connectivity
        sb.Append(embed.Provider).Append('|')
          .Append(embed.ModelId).Append('|')
          .Append(embed.Endpoint).Append('|')
          .Append(embed.ApiKey?.Reveal() ?? "null").Append('|');

        // 4. SK Specific Infrastructure
        sb.Append(aiskOptions.OrgId).Append('|')
          .Append(aiskOptions.DeploymentName).Append('|')
          .Append(aiskOptions.TemplateFormat).Append('|')
          .Append(aiskOptions.EnableNativePlanners).Append('|');

        // 5. Plugins Feature Set (Deterministic ordering)
        sb.Append(aiskOptions.Plugins.AutoDiscoveryEnabled).Append('|');

        if (aiskOptions.Plugins.Types.Count > 0)
        {
            var sortedTypes = aiskOptions.Plugins.Types.OrderBy(x => x.Key);
            foreach (var kvp in sortedTypes)
            {
                sb.Append(kvp.Key).Append(':').Append(kvp.Value).Append(';');
            }
        }

        if (aiskOptions.Plugins.AssembliesToScan.Count > 0)
        {
            var sortedAssemblies = aiskOptions.Plugins.AssembliesToScan.OrderBy(x => x);
            foreach (var asm in sortedAssemblies)
            {
                sb.Append(asm).Append(';');
            }
        }

        // 6. Secure Hashing (SHA256)
        // Ensure the raw ApiKey never leaves this method scope
        byte[] inputBytes = Encoding.UTF8.GetBytes(sb.ToString());
        byte[] hashBytes = SHA256.HashData(inputBytes);

        return "AISK|K|" + Convert.ToHexString(hashBytes);
    }
}
