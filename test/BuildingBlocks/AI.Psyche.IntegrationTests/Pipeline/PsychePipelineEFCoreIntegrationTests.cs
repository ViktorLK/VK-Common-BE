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
using Xunit;

namespace VK.Blocks.AI.Psyche.IntegrationTests.Pipeline;

/// <summary>
/// Integration tests verifying the end-to-end cognitive prompt weaving and dialogue execution pipeline
/// against a real EFCore SQLite database and real DI container.
/// Follows AP.01 (sealed class default), CS.01, CS.03, CS.06, CS.08, and DL.01.
/// </summary>
public sealed class PsychePipelineEFCoreIntegrationTests : PsycheIntegrationTestBase
{
    [Fact]
    public async Task ExecuteAsync_WeaveOnly_AssemblesPromptFromDatabaseWithoutChatEngine()
    {
        // [DL.01] Happy Path: WeaveOnly pipeline run assembling prompt from Persona & Directive in SQLite
        // Arrange
        var tenantId = VKTenantId.New(GuidGenerator);
        TenantProvider.CurrentTenantId = tenantId;
        var personaId = VKPersonaId.New(GuidGenerator);
        var directiveId = VKDirectiveId.New(GuidGenerator);
        var sessionId = VKSessionId.New(GuidGenerator);
        var now = DateTimeOffset.UtcNow;

        await using (var db = CreateDbContext())
        {
            db.Personas.Add(new VKPsychePersonaEntity
            {
                TenantId = tenantId,
                Id = personaId,
                Name = "IntegrationArchitect",
                Description = "You are a master software architect specialized in industrial resilience.",
                Priority = 1,
                CreatedAt = now
            });

            db.Directives.Add(new VKPsycheDirectiveEntity
            {
                TenantId = tenantId,
                Id = directiveId,
                Overview = "Global security protocol: prioritize data integrity and thread safety.",
                BehaviorRules = "Always emit deterministic, structured responses.",
                Priority = 1,
                CreatedAt = now
            });

            db.Sessions.Add(new VKPsycheSessionEntity
            {
                TenantId = tenantId,
                Id = sessionId,
                Status = VKSessionStatus.Active,
                Mode = VKSessionMode.Isolated,
                TurnCount = 0,
                CreatedAt = now
            });

            await db.SaveChangesAsync();
        }

        ChatEngine.ClearBatches();
        using var scope = CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<IVKPsychePipeline>();

        var request = new VKPsycheRequest
        {
            UserInput = "How should we design cross-aggregate boundaries?",
            SessionId = sessionId,
            PersonaId = personaId,
            DirectiveIds = [directiveId],
            WeaveOnly = true
        };

        // Act
        var result = await pipeline.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Messages.Should().NotBeEmpty();

        var systemPrompt = string.Join("\n", result.Value.Messages
            .Where(m => m.Role == VKChatRole.System)
            .Select(m => m.Content));

        systemPrompt.Should().Contain("master software architect");
        systemPrompt.Should().Contain("Global security protocol");

        // WeaveOnly must NOT invoke the downstream chat engine or save echoes
        ChatEngine.ReceivedMessageBatches.Should().BeEmpty();

        await using (var verifyDb = CreateDbContext())
        {
            var echoCount = await verifyDb.Echoes.CountAsync(e => e.SessionId == sessionId);
            echoCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task ExecuteAsync_FullTurnExecution_PersistsEchoesAndUpdatesSessionTurnCount()
    {
        // [DL.01] Happy Path: Full dialogue turn executes chat engine, persists echoes, and increments session turn count
        // Arrange
        var tenantId = VKTenantId.New(GuidGenerator);
        TenantProvider.CurrentTenantId = tenantId;
        var sessionId = VKSessionId.New(GuidGenerator);
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
                CreatedAt = now
            });
            await db.SaveChangesAsync();
        }

        ChatEngine.ClearBatches();
        ChatEngine.ReplyContent = "Follow the CQRS and Event Sourcing pattern for clean aggregates.";

        using var scope = CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<IVKPsychePipeline>();

        var request = new VKPsycheRequest
        {
            UserInput = "What pattern should I use for DDD state changes?",
            SessionId = sessionId,
            WeaveOnly = false
        };

        // Act
        var result = await pipeline.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ChatResponse.Should().NotBeNull();
        result.Value.ChatResponse!.Message.Content.Should().Be("Follow the CQRS and Event Sourcing pattern for clean aggregates.");

        // Verify downstream chat engine invocation
        ChatEngine.ReceivedMessageBatches.Should().HaveCount(1);

        // Verify SQLite database persistence for Echoes and Session update
        await using (var verifyDb = CreateDbContext())
        {
            var echoes = await verifyDb.Echoes
                .Where(e => e.SessionId == sessionId)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();

            echoes.Should().HaveCount(2);
            echoes[0].Role.Should().Be(VKChatRole.User);
            echoes[0].Content.Should().Be("What pattern should I use for DDD state changes?");
            echoes[1].Role.Should().Be(VKChatRole.Assistant);
            echoes[1].Content.Should().Be("Follow the CQRS and Event Sourcing pattern for clean aggregates.");

            var session = await verifyDb.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            session.Should().NotBeNull();
            session!.TurnCount.Should().Be(1);
            session.LastActivityAt.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ExecuteAsync_MultiTurnConversation_ExtractsPreviousEchoesFromDatabase()
    {
        // [DL.01] Happy Path: Multi-turn interaction retrieves previous turn echoes from SQLite store into next prompt
        // Arrange
        var tenantId = VKTenantId.New(GuidGenerator);
        TenantProvider.CurrentTenantId = tenantId;
        var sessionId = VKSessionId.New(GuidGenerator);
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
                CreatedAt = now
            });
            await db.SaveChangesAsync();
        }

        ChatEngine.ClearBatches();

        // Turn 1
        using (var scope1 = CreateScope())
        {
            var pipeline = scope1.ServiceProvider.GetRequiredService<IVKPsychePipeline>();
            ChatEngine.ReplyContent = "I acknowledge that your team code name is Falcon.";

            var request1 = new VKPsycheRequest
            {
                UserInput = "My team code name is Falcon.",
                SessionId = sessionId,
                WeaveOnly = false
            };

            var result1 = await pipeline.ExecuteAsync(request1);
            result1.IsSuccess.Should().BeTrue();
        }

        // Turn 2
        using (var scope2 = CreateScope())
        {
            var pipeline = scope2.ServiceProvider.GetRequiredService<IVKPsychePipeline>();
            ChatEngine.ReplyContent = "Your team code name is Falcon.";

            var request2 = new VKPsycheRequest
            {
                UserInput = "What is my team code name?",
                SessionId = sessionId,
                WeaveOnly = false
            };

            var result2 = await pipeline.ExecuteAsync(request2);
            result2.IsSuccess.Should().BeTrue();
        }

        // Assert
        // Turn 2 must have sent messages containing Turn 1 dialogue history from SQLite EchoStore
        ChatEngine.ReceivedMessageBatches.Should().HaveCount(2);
        var turn2SentMessages = ChatEngine.ReceivedMessageBatches[1];

        turn2SentMessages.Any(m => m.Content.Contains("My team code name is Falcon.")).Should().BeTrue();
        turn2SentMessages.Any(m => m.Content.Contains("I acknowledge that your team code name is Falcon.")).Should().BeTrue();
        turn2SentMessages.Last().Content.Should().Be("What is my team code name?");

        // Total 4 echoes stored in SQLite
        await using (var verifyDb = CreateDbContext())
        {
            var allEchoes = await verifyDb.Echoes
                .Where(e => e.SessionId == sessionId)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();

            allEchoes.Should().HaveCount(4);

            var session = await verifyDb.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            session.Should().NotBeNull();
            session!.TurnCount.Should().Be(2);
        }
    }

    [Fact]
    public async Task ExecuteAsync_InactiveSession_ReturnsSessionNotActiveError()
    {
        // [DL.01] State/Permission Boundary: Resolving an archived session returns VKSessionErrors.SessionNotActive failure
        // Arrange
        var tenantId = VKTenantId.New(GuidGenerator);
        TenantProvider.CurrentTenantId = tenantId;
        var sessionId = VKSessionId.New(GuidGenerator);
        var now = DateTimeOffset.UtcNow;

        await using (var db = CreateDbContext())
        {
            db.Sessions.Add(new VKPsycheSessionEntity
            {
                TenantId = tenantId,
                Id = sessionId,
                Status = VKSessionStatus.Archived,
                Mode = VKSessionMode.Isolated,
                TurnCount = 5,
                CreatedAt = now
            });
            await db.SaveChangesAsync();
        }

        using var scope = CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<IVKPsychePipeline>();

        var request = new VKPsycheRequest
        {
            UserInput = "Can I continue this closed conversation?",
            SessionId = sessionId,
            WeaveOnly = false
        };

        // Act
        var result = await pipeline.ExecuteAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKSessionErrors.SessionNotActive);
    }
}
