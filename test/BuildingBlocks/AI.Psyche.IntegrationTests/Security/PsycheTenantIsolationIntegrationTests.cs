using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.AI.Psyche.EFCore;
using VK.Blocks.AI.Psyche.IntegrationTests.Fixtures;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using Xunit;

namespace VK.Blocks.AI.Psyche.IntegrationTests.Security;

/// <summary>
/// Integration tests verifying strict multi-tenant boundary isolation across
/// Psyche EFCore persistence entities, repository operations, and pipeline executions.
/// Follows AP.01 (sealed class default), CS.01, CS.03, CS.08, OR.02, and DL.01.
/// </summary>
public sealed class PsycheTenantIsolationIntegrationTests : PsycheIntegrationTestBase
{
    [Fact]
    public async Task Query_GlobalFilter_EnforcesMultiTenantBoundaryAcrossPsycheEntities()
    {
        // [DL.01] Permission/Security Boundary: Global Query Filter strictly hides cross-tenant entities
        // Arrange
        var tenantA = VKTenantId.New(GuidGenerator);
        var tenantB = VKTenantId.New(GuidGenerator);
        var now = DateTimeOffset.UtcNow;

        var personaA = VKPersonaId.New(GuidGenerator);
        var personaB = VKPersonaId.New(GuidGenerator);
        var directiveA = VKDirectiveId.New(GuidGenerator);
        var directiveB = VKDirectiveId.New(GuidGenerator);
        var sessionA = VKSessionId.New(GuidGenerator);
        var sessionB = VKSessionId.New(GuidGenerator);
        var echoA = VKEchoId.New(GuidGenerator);
        var echoB = VKEchoId.New(GuidGenerator);

        // Seed Tenant A
        TenantProvider.CurrentTenantId = tenantA;
        await using (var db = CreateDbContext())
        {
            db.Personas.Add(new VKPsychePersonaEntity { TenantId = tenantA, Id = personaA, Name = "Agent-A", Description = "Desc A", Priority = 1, CreatedAt = now });
            db.Directives.Add(new VKPsycheDirectiveEntity { TenantId = tenantA, Id = directiveA, Overview = "Directive A", Priority = 1, CreatedAt = now });
            db.Sessions.Add(new VKPsycheSessionEntity { TenantId = tenantA, Id = sessionA, Status = VKSessionStatus.Active, CreatedAt = now });
            db.Echoes.Add(new VKPsycheEchoEntity { TenantId = tenantA, Id = echoA, SessionId = sessionA, Content = "Secret payload Tenant A", CreatedAt = now });
            await db.SaveChangesAsync();
        }

        // Seed Tenant B
        TenantProvider.CurrentTenantId = tenantB;
        await using (var db = CreateDbContext())
        {
            db.Personas.Add(new VKPsychePersonaEntity { TenantId = tenantB, Id = personaB, Name = "Agent-B", Description = "Desc B", Priority = 1, CreatedAt = now });
            db.Directives.Add(new VKPsycheDirectiveEntity { TenantId = tenantB, Id = directiveB, Overview = "Directive B", Priority = 1, CreatedAt = now });
            db.Sessions.Add(new VKPsycheSessionEntity { TenantId = tenantB, Id = sessionB, Status = VKSessionStatus.Active, CreatedAt = now });
            db.Echoes.Add(new VKPsycheEchoEntity { TenantId = tenantB, Id = echoB, SessionId = sessionB, Content = "Secret payload Tenant B", CreatedAt = now });
            await db.SaveChangesAsync();
        }

        // Act & Assert for Tenant A
        TenantProvider.CurrentTenantId = tenantA;
        await using (var dbA = CreateDbContext())
        {
            var personas = await dbA.Personas.ToListAsync();
            personas.Should().ContainSingle(p => p.Id == personaA);
            personas.Any(p => p.Id == personaB).Should().BeFalse();

            var directives = await dbA.Directives.ToListAsync();
            directives.Should().ContainSingle(d => d.Id == directiveA);
            directives.Any(d => d.Id == directiveB).Should().BeFalse();

            var sessions = await dbA.Sessions.ToListAsync();
            sessions.Should().ContainSingle(s => s.Id == sessionA);
            sessions.Any(s => s.Id == sessionB).Should().BeFalse();

            var echoes = await dbA.Echoes.ToListAsync();
            echoes.Should().ContainSingle(e => e.Id == echoA);
            echoes.Any(e => e.Id == echoB).Should().BeFalse();
        }

        // Act & Assert for Tenant B
        TenantProvider.CurrentTenantId = tenantB;
        await using (var dbB = CreateDbContext())
        {
            var personas = await dbB.Personas.ToListAsync();
            personas.Should().ContainSingle(p => p.Id == personaB);
            personas.Any(p => p.Id == personaA).Should().BeFalse();

            var directives = await dbB.Directives.ToListAsync();
            directives.Should().ContainSingle(d => d.Id == directiveB);
            directives.Any(d => d.Id == directiveA).Should().BeFalse();

            var sessions = await dbB.Sessions.ToListAsync();
            sessions.Should().ContainSingle(s => s.Id == sessionB);
            sessions.Any(s => s.Id == sessionA).Should().BeFalse();

            var echoes = await dbB.Echoes.ToListAsync();
            echoes.Should().ContainSingle(e => e.Id == echoB);
            echoes.Any(e => e.Id == echoA).Should().BeFalse();
        }
    }

    [Fact]
    public async Task FindByIdAsync_CrossTenantAggregateRepository_ReturnsEntityNotFound()
    {
        // [DL.01] Permission/Security Boundary: Strongly-typed Aggregate Repositories cannot access other tenants
        // Arrange
        var tenantA = VKTenantId.New(GuidGenerator);
        var tenantB = VKTenantId.New(GuidGenerator);
        var sessionA = VKSessionId.New(GuidGenerator);
        var now = DateTimeOffset.UtcNow;

        TenantProvider.CurrentTenantId = tenantA;
        await using (var db = CreateDbContext())
        {
            db.Sessions.Add(new VKPsycheSessionEntity
            {
                TenantId = tenantA,
                Id = sessionA,
                Status = VKSessionStatus.Active,
                Mode = VKSessionMode.Isolated,
                TurnCount = 3,
                CreatedAt = now
            });
            await db.SaveChangesAsync();
        }

        // Act: Attempt to access Tenant A's session using Tenant B context
        TenantProvider.CurrentTenantId = tenantB;
        using var scope = CreateScope();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<IVKPsycheSessionRepository>();

        var result = await sessionRepository.FindByIdAsync(sessionA);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKPersistenceErrors.Repository.EntityNotFound);
    }

    [Fact]
    public async Task ExecuteAsync_CrossTenantSessionAccess_IsolatesEchoHistoryInPipeline()
    {
        // [DL.01] Permission/Security Boundary: Pipeline execution under Tenant B cannot leak Tenant A dialogue history
        // Arrange
        var tenantA = VKTenantId.New(GuidGenerator);
        var tenantB = VKTenantId.New(GuidGenerator);
        var sessionA = VKSessionId.New(GuidGenerator);
        var now = DateTimeOffset.UtcNow;

        // Tenant A creates session and records a confidential turn
        TenantProvider.CurrentTenantId = tenantA;
        await using (var db = CreateDbContext())
        {
            db.Sessions.Add(new VKPsycheSessionEntity
            {
                TenantId = tenantA,
                Id = sessionA,
                Status = VKSessionStatus.Active,
                CreatedAt = now
            });
            db.Echoes.Add(new VKPsycheEchoEntity
            {
                TenantId = tenantA,
                Id = VKEchoId.New(GuidGenerator),
                SessionId = sessionA,
                Role = VKChatRole.User,
                Content = "Tenant A Confidential Business Secret: Project Zephyr",
                CreatedAt = now
            });
            await db.SaveChangesAsync();
        }

        // Act: Tenant B attempts to execute pipeline pointing to Tenant A's session
        TenantProvider.CurrentTenantId = tenantB;
        ChatEngine.ClearBatches();

        using var scope = CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<IVKPsychePipeline>();

        var request = new VKPsycheRequest
        {
            UserInput = "Summarize the project status.",
            SessionId = sessionA,
            WeaveOnly = false
        };

        var result = await pipeline.ExecuteAsync(request);

        // Assert: Pipeline fails fast and never invokes ChatEngine on unauthenticated/cross-tenant session access
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(VKPersistenceErrors.Repository.EntityNotFound);
        ChatEngine.ReceivedMessageBatches.Should().BeEmpty();
    }
}
