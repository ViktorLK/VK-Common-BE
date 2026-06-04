using Microsoft.AspNetCore.Http;
using VK.Blocks.AI.SemanticKernel;
using VK.Labs.PersonaWeavePulsar.Echo;

namespace VK.Labs.PersonaWeavePulsar.DependencyInjection.Internal;

/// <summary>
/// PWP-specific AI options provider implementing the L1-L4 hierarchy.
/// P0: Args (via PwpContext)
/// P1: Context (via Headers)
/// P2: Database (via SessionStore)
/// P3: AppSettings (via DefaultProvider)
/// </summary>
internal sealed class PwpAISKOptionsProvider(
    IHttpContextAccessor httpContextAccessor,
    VKAISKDefaultOptionsProvider defaultProvider,
    PwpContext pwpContext,
    IPwpChatHistoryStore sessionStore) : IVKAISKOptionsProvider
{
    public VKAISKOptions GetOptions()
    {
        // L4: System Default (P3)
        var options = defaultProvider.GetOptions();

        // L3: Database (P2) - Persona/Session level
        string? sessionId = pwpContext.SessionId ?? httpContextAccessor.HttpContext?.Request.Headers["X-PWP-Session-Id"];
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            // Note: Sync-over-async in DI is generally discouraged but common in OptionsProviders.
            // For a Lab using SQLite, the impact is minimal.
            var sessionResult = sessionStore.GetSessionAsync(sessionId).GetAwaiter().GetResult();
            if (sessionResult.IsSuccess)
            {
                var s = sessionResult.Value;
                // Connection settings (ModelId, ApiKey, etc.) are now delegated to feature-specific options.
                // VKAISKOptions only contains SK-specific settings.
            }
        }

        // L2: Request Context (P1) - Header overrides
        var context = httpContextAccessor.HttpContext;
        if (context != null)
        {
            // Connection overrides should be handled by a dedicated provider or feature-specific options.
        }

        // L1: Method Args (P0) - Highest priority
        if (pwpContext.OverrideOptions != null)
        {
            var p0 = pwpContext.OverrideOptions;
            options = options with
            {
                TemplateFormat = p0.TemplateFormat != AISKTemplateFormat.Default ? p0.TemplateFormat : options.TemplateFormat,
                EnableNativePlanners = p0.EnableNativePlanners
            };
        }

        return options;
    }
}
