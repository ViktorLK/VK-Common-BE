using System;
using System.Security.Cryptography;
using System.Text;
using VK.Blocks.Core;

namespace VK.Blocks.Workflow.Idempotency.Internal;

/// <summary>
/// Default SHA256-based deterministic idempotency key generator.
/// Follows AP.01.
/// </summary>
internal sealed class DefaultIdempotencyKeyGenerator : IVKIdempotencyKeyGenerator
{
    private readonly IVKJsonSerializer _jsonSerializer;

    public DefaultIdempotencyKeyGenerator(IVKJsonSerializer jsonSerializer)
    {
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
    }

    public string GenerateKey<TContext>(string workflowName, TContext context)
    {
        VKGuard.NotNullOrWhiteSpace(workflowName);
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var payloadJson = _jsonSerializer.Serialize(context);
        var input = $"{workflowName}:{payloadJson}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
