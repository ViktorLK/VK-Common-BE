using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.Psyche;
using VK.Blocks.AI.Psyche.EFCore;
using VK.Blocks.AI.Psyche.IntegrationTests.Fixtures;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.IntegrationTests.Database;

/// <summary>
/// Integration tests verifying optimistic concurrency control (IVKConcurrency / RowVersion)
/// and entity lifecycle integrity across Psyche EFCore persistence models.
/// Follows AP.01 (sealed class default), CS.01, CS.03, CS.08, and DL.01.
/// </summary>
public sealed class PsycheConcurrencyIntegrationTests : PsycheIntegrationTestBase
{
    [Fact]
    public async Task SaveChangesAsync_ConcurrentSessionUpdates_ThrowsDbUpdateConcurrencyException()
    {
        // [DL.01] Concurrency & Conflict: Stale RowVersion throws DbUpdateConcurrencyException
        // Arrange
        var tenantId = VKTenantId.New(GuidGenerator);
        TenantProvider.CurrentTenantId = tenantId;
        var sessionId = VKSessionId.New(GuidGenerator);
        var now = DateTimeOffset.UtcNow;
        byte[] initialRowVersion = [1, 0, 0, 0];

        await using (var seedDb = CreateDbContext())
        {
            seedDb.Sessions.Add(new VKPsycheSessionEntity
            {
                TenantId = tenantId,
                Id = sessionId,
                Status = VKSessionStatus.Active,
                Mode = VKSessionMode.Isolated,
                TurnCount = 1,
                RowVersion = initialRowVersion,
                CreatedAt = now
            });
            await seedDb.SaveChangesAsync();
        }

        // Open two separate DbContext instances simulating concurrent requests
        await using var dbContext1 = CreateDbContext();
        await using var dbContext2 = CreateDbContext();

        var sessionInContext1 = await dbContext1.Sessions.FirstAsync(s => s.Id == sessionId);
        var sessionInContext2 = await dbContext2.Sessions.FirstAsync(s => s.Id == sessionId);

        // Act: Context 1 updates first and updates the RowVersion
        sessionInContext1.TurnCount = 2;
        sessionInContext1.RowVersion = [2, 0, 0, 0];
        await dbContext1.SaveChangesAsync();

        // Context 2 tries to update with stale RowVersion [1, 0, 0, 0]
        sessionInContext2.TurnCount = 5;
        sessionInContext2.RowVersion = [3, 0, 0, 0];

        Func<Task> concurrentSaveAct = async () => await dbContext2.SaveChangesAsync();

        // Assert: EF Core detects that the database RowVersion no longer matches context 2's original [1, 0, 0, 0]
        await concurrentSaveAct.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Fact]
    public async Task UpdateAsync_AggregateRepository_HandlesConcurrencyConflictGracefully()
    {
        // [DL.01] Concurrency Boundary: Aggregate repository returns failure when optimistic concurrency fails
        // Arrange
        var tenantId = VKTenantId.New(GuidGenerator);
        TenantProvider.CurrentTenantId = tenantId;
        var sessionId = VKSessionId.New(GuidGenerator);
        var now = DateTimeOffset.UtcNow;
        byte[] initialRowVersion = [1, 0, 0, 0];

        await using (var seedDb = CreateDbContext())
        {
            seedDb.Sessions.Add(new VKPsycheSessionEntity
            {
                TenantId = tenantId,
                Id = sessionId,
                Status = VKSessionStatus.Active,
                Mode = VKSessionMode.Isolated,
                TurnCount = 1,
                RowVersion = initialRowVersion,
                CreatedAt = now
            });
            await seedDb.SaveChangesAsync();
        }

        using var scope1 = CreateScope();
        using var scope2 = CreateScope();

        var repo1 = scope1.ServiceProvider.GetRequiredService<IVKPsycheSessionRepository>();
        var repo2 = scope2.ServiceProvider.GetRequiredService<IVKPsycheSessionRepository>();

        var session1 = (await repo1.FindByIdAsync(sessionId)).Value!;
        var session2 = (await repo2.FindByIdAsync(sessionId)).Value!;

        // Pre-load tracked entity into scope2 ChangeTracker before scope1 commits, simulating concurrent in-flight transaction
        var db2 = scope2.ServiceProvider.GetRequiredService<PsycheIntegrationDbContext>();
        _ = await db2.Sessions.FirstAsync(s => s.Id == sessionId);

        // Act: Scope 1 successfully increments turn and commits
        session1.IncrementTurn(now.AddSeconds(1));
        session1.RowVersion = [2, 0, 0, 0];
        var update1Result = await repo1.UpdateAsync(session1);
        update1Result.IsSuccess.Should().BeTrue();

        // Scope 2 tries to update with stale RowVersion [1, 0, 0, 0]
        session2.IncrementTurn(now.AddSeconds(2));
        session2.RowVersion = [3, 0, 0, 0];
        var update2Result = await repo2.UpdateAsync(session2);

        // Assert: Repository catches concurrency error and returns Failure
        update2Result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SaveChangesAsync_EntityLifecycleState_PreservesAuditTimestampsAndTenantCoordinates()
    {
        // [DL.01] Happy Path: Verifies that session, persona, and directive entity states persist accurately in SQLite
        // Arrange
        var tenantId = VKTenantId.New(GuidGenerator);
        TenantProvider.CurrentTenantId = tenantId;
        var sessionId = VKSessionId.New(GuidGenerator);
        var directiveId = VKDirectiveId.New(GuidGenerator);
        var now = DateTimeOffset.UtcNow;

        await using (var db = CreateDbContext())
        {
            db.Sessions.Add(new VKPsycheSessionEntity
            {
                TenantId = tenantId,
                Id = sessionId,
                Status = VKSessionStatus.Active,
                Mode = VKSessionMode.Isolated,
                TurnCount = 0,
                LastActivityAt = now,
                CreatedAt = now
            });

            db.Directives.Add(new VKPsycheDirectiveEntity
            {
                TenantId = tenantId,
                Id = directiveId,
                Overview = "Core System Directives",
                Priority = 5,
                CreatedAt = now
            });

            await db.SaveChangesAsync();
        }

        // Act & Assert
        await using (var verifyDb = CreateDbContext())
        {
            var session = await verifyDb.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            session.Should().NotBeNull();
            session!.TenantId.Should().Be(tenantId);
            session.Status.Should().Be(VKSessionStatus.Active);
            session.TurnCount.Should().Be(0);
            session.CreatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
            session.LastActivityAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));

            var directive = await verifyDb.Directives.FirstOrDefaultAsync(d => d.Id == directiveId);
            directive.Should().NotBeNull();
            directive!.TenantId.Should().Be(tenantId);
            directive.Overview.Should().Be("Core System Directives");
            directive.Priority.Should().Be(5);
        }
    }
}
