using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Labs.PersonaWeavePulsar.Echo;
using VK.Labs.PersonaWeavePulsar.TenantConfig;

namespace VK.Labs.PersonaWeavePulsar.DependencyInjection.Internal;

/// <summary>
/// PWP-specific Chat options provider implementing the L1-L4 hierarchy.
/// Handles dynamic overrides for ApiKey, ModelId, etc. via Headers, Database, and Context.
/// </summary>
internal sealed class PwpChatOptionsProvider(
    IHttpContextAccessor httpContextAccessor,
    IOptions<VKChatOptions> defaultOptions,
    PwpContext pwpContext,
    IPwpTenantConfigStore configStore,
    IPwpChatHistoryStore sessionStore) : IVKChatOptionsProvider
{
    public VKChatOptions GetOptions()
    {
        // L4: System Default (P3)
        var options = defaultOptions.Value;
        Console.WriteLine($"[PWP] Default Chat ApiKey is NULL: {options.ApiKey == null}");
        if (options.ApiKey != null)
        {
            Console.WriteLine($"[PWP] Default Chat ApiKey Empty: {options.ApiKey.Value.IsEmpty}");
        }

        // L3.5: Database (Global) - UI-level defaults
        var globalConfigResult = configStore.GetTenantConfigAsync().GetAwaiter().GetResult();
        if (globalConfigResult.IsSuccess)
        {
            var g = globalConfigResult.Value;
            options = options with
            {
                Provider = g.DefaultProvider ?? options.Provider,
                ModelId = g.DefaultModelId ?? options.ModelId,
                ApiKey = g.DefaultApiKey != null ? new VK.Blocks.Core.VKSensitiveString(g.DefaultApiKey) : options.ApiKey,
                Endpoint = g.DefaultEndpoint ?? options.Endpoint
            };
        }

        // L3: Database (P2) - Persona/Session level
        string? sessionId = pwpContext.SessionId ?? httpContextAccessor.HttpContext?.Request.Headers["X-PWP-Session-Id"];
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var sessionResult = sessionStore.GetSessionAsync(sessionId).GetAwaiter().GetResult();
            if (sessionResult.IsSuccess)
            {
                var s = sessionResult.Value;
                options = options with
                {
                    ModelId = s.CustomModelId ?? options.ModelId,
                    ApiKey = s.CustomApiKey != null ? new VK.Blocks.Core.VKSensitiveString(s.CustomApiKey) : options.ApiKey,
                    Provider = s.CustomServiceType != null && System.Enum.TryParse<VKAIProviderType>(s.CustomServiceType, true, out var p) ? p : options.Provider,
                    Endpoint = s.CustomEndpoint ?? options.Endpoint
                };
            }
        }

        // L2: Request Context (P1) - Header overrides
        var context = httpContextAccessor.HttpContext;
        if (context != null)
        {
            string? hModelId = context.Request.Headers["X-PWP-Model-Id"];
            string? hApiKey = context.Request.Headers["X-PWP-Api-Key"];
            string? hServiceType = context.Request.Headers["X-PWP-Service-Type"];
            string? hEndpoint = context.Request.Headers["X-PWP-Endpoint"];

            options = options with
            {
                ModelId = !string.IsNullOrWhiteSpace(hModelId) ? hModelId : options.ModelId,
                ApiKey = !string.IsNullOrWhiteSpace(hApiKey) ? new VK.Blocks.Core.VKSensitiveString(hApiKey) : options.ApiKey,
                Provider = !string.IsNullOrWhiteSpace(hServiceType) && System.Enum.TryParse<VKAIProviderType>(hServiceType, true, out var p) ? p : options.Provider,
                Endpoint = !string.IsNullOrWhiteSpace(hEndpoint) ? hEndpoint : options.Endpoint
            };
        }

        // L1: Method Args (P0) - Highest priority
        if (pwpContext.OverrideChatOptions != null)
        {
            var p0 = pwpContext.OverrideChatOptions;
            options = options with
            {
                ModelId = p0.ModelId ?? options.ModelId,
                ApiKey = p0.ApiKey ?? options.ApiKey,
                Provider = p0.Provider ?? options.Provider,
                Endpoint = p0.Endpoint ?? options.Endpoint,
                Temperature = p0.Temperature ?? options.Temperature
            };
        }

        return options;
    }
}
