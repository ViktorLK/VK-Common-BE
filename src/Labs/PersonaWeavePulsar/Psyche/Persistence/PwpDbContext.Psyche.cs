using Microsoft.EntityFrameworkCore;
using VK.Labs.PersonaWeavePulsar.Psyche.Directive.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Echo.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Persona.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Profile.Entities;
using VK.Labs.PersonaWeavePulsar.Psyche.Session.Entities;

namespace VK.Labs.PersonaWeavePulsar.Persistence;

public sealed partial class PwpDbContext
{
    public DbSet<PwpPersonaEntity> Personas => Set<PwpPersonaEntity>();
    public DbSet<PwpKnowledgeEntity> KnowledgeEntries => Set<PwpKnowledgeEntity>();
    public DbSet<PwpKnowledgeKeyEntity> KnowledgeKeys => Set<PwpKnowledgeKeyEntity>();
    public DbSet<PwpSessionEntity> ChatSessions => Set<PwpSessionEntity>();
    public DbSet<PwpEchoEntity> ChatMessages => Set<PwpEchoEntity>();
    public DbSet<PwpDirectiveEntity> Directives => Set<PwpDirectiveEntity>();
    public DbSet<PwpPatternEntity> Patterns => Set<PwpPatternEntity>();
    public DbSet<PwpProfileEntity> ProfilePresences => Set<PwpProfileEntity>();
}
