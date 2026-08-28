using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
/// Provides EFCore-backed implementations for all AI.Psyche stores and auto-generated entity repositories.
/// Follows BB.02, AP.01, AP.02.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution and metadata; contains no business logic.")]
[VKBlockMarker(Dependencies = [typeof(VKAIPsycheBlock), typeof(VKPersistenceEFCoreBlock)], Toggleable = false)]
public sealed partial class VKAIPsycheEFCoreBlock
{
    static partial void RegisterBlockCustom(IVKAIPsycheEFCoreBuilder builder)
    {
        var services = builder.Services;

        // AI.Psyche Domain Repositories & Stores (EFCore Backed)
        services.TryAddScoped<EFCoreDirectiveRepository>();
        services.TryAddScoped<IVKDirectiveRepository>(sp => sp.GetRequiredService<EFCoreDirectiveRepository>());
        services.TryAddScoped<IVKDirectiveStore>(sp => sp.GetRequiredService<EFCoreDirectiveRepository>());
        services.TryAddScoped<IVKReadRepository<VKDirectiveCharter, VKDirectiveId>>(sp => sp.GetRequiredService<EFCoreDirectiveRepository>());
        services.TryAddScoped<IVKKnowledgeStore, KnowledgeStore>();
        services.TryAddScoped<IVKPatternStore, PatternStore>();
        services.TryAddScoped<IVKPersonaStore, PersonaStore>();
        services.TryAddScoped<IVKSessionStore, SessionStore>();
        services.TryAddScoped<IVKEchoStore, EchoStore>();
        services.TryAddScoped<IVKProfileStore, ProfileStore>();
    }
}
