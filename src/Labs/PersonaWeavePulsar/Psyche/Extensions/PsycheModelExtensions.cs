using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VK.Blocks.AI.Psyche;
using VK.Labs.PersonaWeavePulsar.Pwp.Preset;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Extensions;

/// <summary>
/// EF Core model convention extensions for AI.Psyche strongly-typed identifiers.
/// </summary>
public static class PwpModelExtensions
{
    /// <summary>
    /// Configures EF Core value conversions for AI.Psyche strongly-typed IDs.
    /// </summary>
    public static void ConfigurePsycheConventions(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<VKEchoId>().HaveConversion<VKEchoIdConverter>();
        configurationBuilder.Properties<VKSessionId>().HaveConversion<VKSessionIdConverter>();
        configurationBuilder.Properties<VKPersonaId>().HaveConversion<VKPersonaIdConverter>();
        configurationBuilder.Properties<VKDirectiveId>().HaveConversion<VKDirectiveIdConverter>();
        configurationBuilder.Properties<VKKnowledgeId>().HaveConversion<VKKnowledgeIdConverter>();
        configurationBuilder.Properties<VKPatternId>().HaveConversion<VKPatternIdConverter>();
        configurationBuilder.Properties<PwpPresetId>().HaveConversion<PwpPresetIdConverter>();
    }

    private sealed class PwpPresetIdConverter() : ValueConverter<PwpPresetId, Guid>(id => id.Value, value => new Pwp.Preset.PwpPresetId(value));
    private sealed class VKEchoIdConverter() : ValueConverter<VKEchoId, Guid>(id => id.Value, value => new VKEchoId(value));
    private sealed class VKSessionIdConverter() : ValueConverter<VKSessionId, Guid>(id => id.Value, value => new VKSessionId(value));
    private sealed class VKPersonaIdConverter() : ValueConverter<VKPersonaId, Guid>(id => id.Value, value => new VKPersonaId(value));
    private sealed class VKDirectiveIdConverter() : ValueConverter<VKDirectiveId, Guid>(id => id.Value, value => new VKDirectiveId(value));
    private sealed class VKKnowledgeIdConverter() : ValueConverter<VKKnowledgeId, Guid>(id => id.Value, value => new VKKnowledgeId(value));
    private sealed class VKPatternIdConverter() : ValueConverter<VKPatternId, Guid>(id => id.Value, value => new VKPatternId(value));
}
