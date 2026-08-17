using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;
using VK.Labs.PersonaWeavePulsar.Psyche.Directive.Repositories;
using VK.Labs.PersonaWeavePulsar.Psyche.Directive.Stores;
using VK.Labs.PersonaWeavePulsar.Psyche.Echo.Repositories;
using VK.Labs.PersonaWeavePulsar.Psyche.Echo.Stores;
using VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Repositories;
using VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Stores;
using VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Repositories;
using VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Stores;
using VK.Labs.PersonaWeavePulsar.Psyche.Persona.Repositories;
using VK.Labs.PersonaWeavePulsar.Psyche.Persona.Stores;
using VK.Labs.PersonaWeavePulsar.Psyche.Profile.Stores;
using VK.Labs.PersonaWeavePulsar.Psyche.Session.Repositories;
using VK.Labs.PersonaWeavePulsar.Psyche.Session.Stores;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Extensions;

/// <summary>
/// Service collection extensions for configuring PWP AI.Psyche persistence and bridges.
/// Autonomously bundles all Psyche store and repository registrations.
/// </summary>
public static class PwpPsycheExtensions
{
    public static IServiceCollection AddPwpPsychePersistence(this IServiceCollection services)
    {
        // 1. Directive Domain
        services.TryAddScoped<IVKDirectiveStore, PwpDirectiveStore>();
        services.TryAddScoped<IPwpDirectiveRepository, PwpDirectiveRepository>();

        // 2. Knowledge Domain
        services.TryAddScoped<IVKKnowledgeStore, PwpKnowledgeStore>();
        services.TryAddScoped<IPwpKnowledgeRepository, PwpKnowledgeRepository>();

        // 3. Pattern Domain
        services.TryAddScoped<IVKPatternStore, PwpPatternStore>();
        services.TryAddScoped<IPwpPatternRepository, PwpPatternRepository>();

        // 4. Persona Domain
        services.TryAddScoped<IVKPersonaStore, PwpPersonaStore>();
        services.TryAddScoped<IPwpPersonaRepository, PwpPersonaRepository>();

        // 5. Session & Echo Domains
        services.TryAddScoped<IVKSessionStore, PwpSessionStore>();
        services.TryAddScoped<IPwpSessionRepository, PwpSessionRepository>();
        services.TryAddScoped<IVKEchoStore, PwpEchoStore>();
        services.TryAddScoped<IPwpEchoRepository, PwpEchoRepository>();

        // 6. Profile Domain
        services.TryAddScoped<IVKProfileStore, PwpProfileStore>();

        return services;
    }
}
