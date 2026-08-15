using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Error constants for the AI.Synapse building block.
/// </summary>
public static class VKAISynapseErrors
{
    public static readonly VKError NoAvailableProvider = new("AISynapse.NoAvailableProvider", "No healthy AI provider is currently available to service the request.");
    public static readonly VKError ProviderNotFound = new("AISynapse.ProviderNotFound", "The specified AI connection or provider was not found.");
    public static readonly VKError RateLimitExceeded = new("AISynapse.RateLimitExceeded", "AI Synapse rate limit or concurrent capacity exceeded for this connection.");
    public static readonly VKError QuotaExceeded = new("AISynapse.QuotaExceeded", "The tenant token budget or quota has been exhausted.");
    public static readonly VKError AllProvidersFailed = new("AISynapse.AllProvidersFailed", "All candidate AI providers failed during execution and fallback.");
    public static readonly VKError InvalidConfiguration = new("AISynapse.InvalidConfiguration", "The AI Synapse connection configuration is invalid.");
    public static readonly VKError InvalidEndpoint = new("AISynapse.Security.InvalidEndpoint", "The requested AI endpoint is invalid or malformed.", VKErrorType.Validation);
    public static readonly VKError UnauthorizedConnection = new("AISynapse.Security.Unauthorized", "The target AI endpoint or BYOK credential is not authorized for this tenant.", VKErrorType.Unauthorized);
}
