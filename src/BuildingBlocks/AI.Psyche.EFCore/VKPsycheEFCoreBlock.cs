using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.EFCore.Common.Internal;
using VK.Blocks.AI.Psyche.EFCore.Directive.Internal;
using VK.Blocks.AI.Psyche.EFCore.Echo.Internal;
using VK.Blocks.AI.Psyche.EFCore.Knowledge.Internal;
using VK.Blocks.AI.Psyche.EFCore.Pattern.Internal;
using VK.Blocks.AI.Psyche.EFCore.Persona.Internal;
using VK.Blocks.AI.Psyche.EFCore.Profile.Internal;
using VK.Blocks.AI.Psyche.EFCore.Session.Internal;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// AI.Psyche.EFCore Building Block Marker.
/// Provides EFCore-backed implementations for all AI.Psyche stores and dedicated entity repositories.
/// Follows BB.02, AP.01, AP.02.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution and metadata; contains no business logic.")]
[VKBlockMarker(Dependencies = [typeof(VKAIPsycheBlock), typeof(VKPersistenceEFCoreBlock)], Toggleable = false)]
public sealed partial class VKAIPsycheEFCoreBlock
{
    static partial void RegisterBlockCustom(IVKAIPsycheEFCoreBuilder builder)
    {
        var services = builder.Services;

        // Model & Convention Contributors (Automatic Zero-Config Registration)
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKModelCreatingContributor, PsycheModelContributor>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKModelConventionContributor, PsycheModelContributor>());

        // 1. Directive Domain
        services.TryAddScoped<IVKDirectiveStore, DirectiveStore>();
        services.TryAddScoped<IVKPsycheDirectiveRepository, DirectiveRepository>();

        // 2. Knowledge Domain
        services.TryAddScoped<IVKKnowledgeStore, KnowledgeStore>();
        services.TryAddScoped<IVKPsycheKnowledgeRepository, KnowledgeRepository>();

        // 3. Pattern Domain
        services.TryAddScoped<IVKPatternStore, PatternStore>();
        services.TryAddScoped<IVKPsychePatternRepository, PatternRepository>();

        // 4. Persona Domain
        services.TryAddScoped<IVKPersonaStore, PersonaStore>();
        services.TryAddScoped<IVKPsychePersonaRepository, PersonaRepository>();

        // 5. Session & Echo Domains
        services.TryAddScoped<IVKSessionStore, SessionStore>();
        services.TryAddScoped<IVKPsycheSessionRepository, SessionRepository>();
        services.TryAddScoped<IVKEchoStore, EchoStore>();
        services.TryAddScoped<IVKPsycheEchoRepository, EchoRepository>();

        // 6. Profile Domain
        services.TryAddScoped<IVKProfileStore, ProfileStore>();
        services.TryAddScoped<IVKPsycheProfileRepository, ProfileRepository>();
    }
}
