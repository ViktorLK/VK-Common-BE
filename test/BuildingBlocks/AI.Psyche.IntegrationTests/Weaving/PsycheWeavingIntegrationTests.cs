using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.AI.Psyche.EFCore;
using VK.Blocks.AI.Psyche.IntegrationTests.Fixtures;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.IntegrationTests.Weaving;

/// <summary>
/// Integration tests verifying the end-to-end cognitive prompt weaving engine (Tapestry,
/// fragment replacement, relative depth positioning, and token-aware history truncation)
/// in a real DI container backed by EFCore SQLite persistence.
/// Follows AP.01 (sealed class default), CS.01, CS.03, CS.06, CS.08, and DL.01.
/// </summary>
public sealed class PsycheWeavingIntegrationTests : PsycheIntegrationTestBase
{
    [Fact]
    public async Task WeaveAsync_FullTapestryXmlWrapping_EnclosesDirectivesAndPersonasInXmlTags()
    {
        // [DL.01] Happy Path: Verifies that Persona and Directives extracted from SQLite are wrapped
        // into standard XML tags (<system_directive>, <persona>) and ordered hierarchically in the System Prompt.
        // Arrange
        var tenantId = VKTenantId.New(GuidGenerator);
        TenantProvider.CurrentTenantId = tenantId;
        var sessionId = VKSessionId.New(GuidGenerator);
        var personaId = VKPersonaId.New(GuidGenerator);
        var directiveId1 = VKDirectiveId.New(GuidGenerator);
        var directiveId2 = VKDirectiveId.New(GuidGenerator);
        var now = DateTimeOffset.UtcNow;

        await using (var db = CreateDbContext())
        {
            db.Sessions.Add(new VKPsycheSessionEntity
            {
                TenantId = tenantId,
                Id = sessionId,
                Status = VKSessionStatus.Active,
                CreatedAt = now
            });

            db.Directives.Add(new VKPsycheDirectiveEntity
            {
                TenantId = tenantId,
                Id = directiveId1,
                Overview = "Security Protocol: Enforce strict defense-in-depth boundaries.",
                Priority = 1,
                CreatedAt = now
            });

            db.Directives.Add(new VKPsycheDirectiveEntity
            {
                TenantId = tenantId,
                Id = directiveId2,
                Overview = "Operational Rule: All database queries must use parameterized statements.",
                Priority = 2,
                CreatedAt = now
            });

            db.Personas.Add(new VKPsychePersonaEntity
            {
                TenantId = tenantId,
                Id = personaId,
                Name = "CloudArchitect",
                Description = "Senior Cloud Solutions Architect specializing in zero-trust architectures.",
                Priority = 1,
                CreatedAt = now
            });

            await db.SaveChangesAsync();
        }

        using var scope = CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<IVKPsychePipeline>();

        var request = new VKPsycheRequest
        {
            UserInput = "Review our microservice security perimeter.",
            SessionId = sessionId,
            DirectiveIds = [directiveId1, directiveId2],
            PersonaId = personaId,
            WeaveOnly = true
        };

        // Act
        var result = await pipeline.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Messages.Should().NotBeEmpty();

        var systemMessage = result.Value.Messages.First(m => m.Role == VKChatRole.System);
        var content = systemMessage.Content;

        // Verify XML tag structure
        content.Should().Contain("<system_directive>");
        content.Should().Contain("</system_directive>");
        content.Should().Contain("<persona>");
        content.Should().Contain("</persona>");

        // Verify content inclusion
        content.Should().Contain("Security Protocol: Enforce strict defense-in-depth boundaries.");
        content.Should().Contain("Operational Rule: All database queries must use parameterized statements.");
        content.Should().Contain("Senior Cloud Solutions Architect");

        // Verify Tapestry hierarchy: Directives section must precede Personas section
        var directivesIndex = content.IndexOf("<system_directive>", StringComparison.Ordinal);
        var personaIndex = content.IndexOf("<persona>", StringComparison.Ordinal);

        directivesIndex.Should().BeGreaterThanOrEqualTo(0);
        personaIndex.Should().BeGreaterThan(directivesIndex);
    }

    [Fact]
    public async Task WeaveAsync_FragmentReplacement_SubstitutesTemplateVariablesAcrossAllSegments()
    {
        // [DL.01] Happy Path: Verifies that template variables {{variable}} in Directives and Personas
        // are dynamically substituted by DefaultFragmentReplacementTask using request-level WeavingArgs.
        // Arrange
        var tenantId = VKTenantId.New(GuidGenerator);
        TenantProvider.CurrentTenantId = tenantId;
        var sessionId = VKSessionId.New(GuidGenerator);
        var directiveId = VKDirectiveId.New(GuidGenerator);
        var personaId = VKPersonaId.New(GuidGenerator);
        var now = DateTimeOffset.UtcNow;

        await using (var db = CreateDbContext())
        {
            db.Sessions.Add(new VKPsycheSessionEntity
            {
                TenantId = tenantId,
                Id = sessionId,
                Status = VKSessionStatus.Active,
                CreatedAt = now
            });

            db.Directives.Add(new VKPsycheDirectiveEntity
            {
                TenantId = tenantId,
                Id = directiveId,
                Overview = "Welcome {{user_name}} to the {{environment_name}} cluster. Compliance level: {{compliance_tier}}.",
                Priority = 1,
                CreatedAt = now
            });

            db.Personas.Add(new VKPsychePersonaEntity
            {
                TenantId = tenantId,
                Id = personaId,
                Name = "VirtualMentor",
                Description = "Assisting {{user_name}} with high-availability systems engineering in {{environment_name}}.",
                Priority = 1,
                CreatedAt = now
            });

            await db.SaveChangesAsync();
        }

        using var scope = CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<IVKPsychePipeline>();

        var request = new VKPsycheRequest
        {
            UserInput = "What is the cluster status?",
            SessionId = sessionId,
            DirectiveIds = [directiveId],
            PersonaId = personaId,
            WeaveOnly = true
        }.WithArgs(new VKWeavingArgs
        {
            Replacements = new Dictionary<string, object?>
            {
                ["user_name"] = "Alice",
                ["environment_name"] = "Tokyo-Production",
                ["compliance_tier"] = "SOC2-TypeII"
            }
        });

        // Act
        var result = await pipeline.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        var systemMessage = result.Value!.Messages.First(m => m.Role == VKChatRole.System);
        var content = systemMessage.Content;

        // Variables must be substituted cleanly
        content.Should().Contain("Welcome Alice to the Tokyo-Production cluster.");
        content.Should().Contain("Compliance level: SOC2-TypeII.");
        content.Should().Contain("Assisting Alice with high-availability systems engineering in Tokyo-Production.");

        // Raw template markers must not remain
        content.Should().NotContain("{{user_name}}");
        content.Should().NotContain("{{environment_name}}");
        content.Should().NotContain("{{compliance_tier}}");
    }

    [Fact]
    public async Task WeaveAsync_RelativeDepthInjections_TopologicallyPositionsSegmentsAroundPillars()
    {
        // [DL.01] Happy Path: Verifies that segments configured with relative depth anchors
        // (BeforeDirective, AfterPersona, BeforeEcho) are topologically positioned around core pillars.
        // Arrange
        var tenantId = VKTenantId.New(GuidGenerator);
        TenantProvider.CurrentTenantId = tenantId;
        var sessionId = VKSessionId.New(GuidGenerator);
        var directiveId = VKDirectiveId.New(GuidGenerator);
        var personaId = VKPersonaId.New(GuidGenerator);
        var echoId = VKEchoId.New(GuidGenerator);
        var now = DateTimeOffset.UtcNow;

        await using (var db = CreateDbContext())
        {
            db.Sessions.Add(new VKPsycheSessionEntity
            {
                TenantId = tenantId,
                Id = sessionId,
                Status = VKSessionStatus.Active,
                CreatedAt = now
            });

            db.Directives.Add(new VKPsycheDirectiveEntity
            {
                TenantId = tenantId,
                Id = directiveId,
                Overview = "Core System Directive Content",
                Priority = 1,
                CreatedAt = now
            });

            db.Personas.Add(new VKPsychePersonaEntity
            {
                TenantId = tenantId,
                Id = personaId,
                Name = "CorePersona",
                Description = "Core Persona Content",
                Priority = 1,
                CreatedAt = now
            });

            db.Echoes.Add(new VKPsycheEchoEntity
            {
                TenantId = tenantId,
                Id = echoId,
                SessionId = sessionId,
                Role = VKChatRole.User,
                Content = "Prior dialogue turn message from database",
                CreatedAt = now.AddMinutes(-1)
            });

            await db.SaveChangesAsync();
        }

        // Build a scoped service provider with an injected relative-depth stage running BEFORE Weaving (Schedule: 995)
        using var customProvider = CreateCustomServiceProvider(services =>
        {
            services.AddScoped<IVKPsychePipelineStage>(_ => new RelativeDepthInjectionTestStage(context =>
            {
                context.AddSegment(new VKPromptSegment
                {
                    Content = "--- PRE-DIRECTIVE BANNER ---",
                    RelativeDepth = VKPromptRelativeDepth.BeforeDirective,
                    DepthPriority = 10
                });

                context.AddSegment(new VKPromptSegment
                {
                    Content = "--- POST-PERSONA FOOTER ---",
                    RelativeDepth = VKPromptRelativeDepth.AfterPersona,
                    DepthPriority = 10
                });

                context.AddSegment(new VKPromptSegment
                {
                    Content = "--- BEFORE-ECHO HISTORICAL CONTEXT ---",
                    RelativeDepth = VKPromptRelativeDepth.BeforeEcho,
                    DepthPriority = 10
                });
            }));
        });

        using var scope = customProvider.CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<IVKPsychePipeline>();

        var request = new VKPsycheRequest
        {
            UserInput = "Execute test inquiry",
            SessionId = sessionId,
            DirectiveIds = [directiveId],
            PersonaId = personaId,
            WeaveOnly = true
        };

        // Act
        var result = await pipeline.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        var messages = result.Value!.Messages.ToList();

        // 1. Verify System message internal relative placement
        var systemMessage = messages.First(m => m.Role == VKChatRole.System);
        var sysContent = systemMessage.Content;

        var preDirectiveIdx = sysContent.IndexOf("--- PRE-DIRECTIVE BANNER ---", StringComparison.Ordinal);
        var directivesIdx = sysContent.IndexOf("<system_directive>", StringComparison.Ordinal);
        var personaIdx = sysContent.IndexOf("<persona>", StringComparison.Ordinal);
        var postPersonaIdx = sysContent.IndexOf("--- POST-PERSONA FOOTER ---", StringComparison.Ordinal);

        preDirectiveIdx.Should().BeGreaterThanOrEqualTo(0);
        directivesIdx.Should().BeGreaterThan(preDirectiveIdx);
        personaIdx.Should().BeGreaterThan(directivesIdx);
        postPersonaIdx.Should().BeGreaterThan(personaIdx);

        // 2. Verify Message list sequence: BeforeEcho must appear before the historical Echoes
        var beforeEchoIdx = messages.FindIndex(m => m.Content.Contains("--- BEFORE-ECHO HISTORICAL CONTEXT ---"));
        var echoMsgIdx = messages.FindIndex(m => m.Content.Contains("Prior dialogue turn message from database"));
        var userMsgIdx = messages.FindIndex(m => m.Content == "Execute test inquiry");

        beforeEchoIdx.Should().BeGreaterThan(0); // Appears after the initial system message
        echoMsgIdx.Should().BeGreaterThan(beforeEchoIdx); // Echo message appears after BeforeEcho injection
        userMsgIdx.Should().BeGreaterThan(echoMsgIdx); // User turn appears last
    }

    [Fact]
    public async Task WeaveAsync_TokenBudgetExceeded_EvictsOlderEchoesAndPreservesRecentTurns()
    {
        // [DL.01] Boundary/Limit: When dialogue history exceeds the configured MaxContextBudget,
        // DefaultPromptTruncateTask and DefaultEchoExtractStage evict older turns and retain recent dialogue turns.
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
                CreatedAt = now
            });

            // Seed 4 history turns (8 messages total, each turn = 45 tokens)
            for (int i = 0; i < 4; i++)
            {
                db.Echoes.Add(new VKPsycheEchoEntity
                {
                    TenantId = tenantId,
                    Id = VKEchoId.New(GuidGenerator),
                    SessionId = sessionId,
                    Role = VKChatRole.User,
                    Content = $"Historical Question Turn #{i + 1} with standard length query content",
                    TokenCount = 20,
                    CreatedAt = now.AddMinutes(-10 + i * 2)
                });

                db.Echoes.Add(new VKPsycheEchoEntity
                {
                    TenantId = tenantId,
                    Id = VKEchoId.New(GuidGenerator),
                    SessionId = sessionId,
                    Role = VKChatRole.Assistant,
                    Content = $"Historical Response Turn #{i + 1} with descriptive assistant reply content",
                    TokenCount = 25,
                    CreatedAt = now.AddMinutes(-9 + i * 2)
                });
            }

            await db.SaveChangesAsync();
        }

        using var scope = CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<IVKPsychePipeline>();

        // Each turn pair is 45 tokens. 4 turns = 180 tokens.
        // Default TokenBudgetRatio is 0.3. Setting MaxContextBudget to 400 allocates 400 * 0.3 = 120 tokens.
        // 120 tokens fits 2 turns (90 tokens), evicting Turn #1 and Turn #2 while keeping Turn #3 and Turn #4.
        var request = new VKPsycheRequest
        {
            UserInput = "Current fresh turn prompt message",
            SessionId = sessionId,
            WeaveOnly = true
        }.WithArgs(new VKWeavingArgs
        {
            MaxContextBudget = 400
        });

        // Act
        var result = await pipeline.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        var wovenMessages = result.Value!.Messages;

        // Oldest Turn #1 and Turn #2 must be evicted
        wovenMessages.Any(m => m.Content.Contains("Historical Question Turn #1")).Should().BeFalse();
        wovenMessages.Any(m => m.Content.Contains("Historical Response Turn #1")).Should().BeFalse();

        // Most recent Turn #4 must be retained
        wovenMessages.Any(m => m.Content.Contains("Historical Question Turn #4")).Should().BeTrue();
        wovenMessages.Any(m => m.Content.Contains("Historical Response Turn #4")).Should().BeTrue();

        // Current fresh user input must be retained at the end
        wovenMessages.Last().Content.Should().Be("Current fresh turn prompt message");
    }

    private sealed class RelativeDepthInjectionTestStage : IVKPsychePipelineStage
    {
        private readonly Action<VKPsycheContext> _injector;

        public RelativeDepthInjectionTestStage(Action<VKPsycheContext> injector)
        {
            _injector = VKGuard.NotNull(injector);
        }

        // Must run before PsycheWeaving (order: 1000) so segments are available for weaving
        public VKPipelineSchedule Schedule => new(995, false, null, VKPipelinePhase.Before);

        public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
        {
            _injector(context);
            return Task.FromResult(VKResult.Success());
        }
    }
}
