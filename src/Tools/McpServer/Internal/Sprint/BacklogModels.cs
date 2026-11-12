using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace VK.Tools.McpServer.Internal.Backlogs;

internal static class McpBacklogErrors
{
    public const string ItemNotFound = "Backlog.Item.NotFound";
    public const string ItemDuplicateId = "Backlog.Item.DuplicateId";
    public const string TransitionInvalid = "Backlog.Transition.Invalid";
    public const string TransitionDodIncomplete = "Backlog.Transition.DodIncomplete";
    public const string SprintAlreadyActive = "Backlog.Sprint.AlreadyActive";
    public const string SprintNoneActive = "Backlog.Sprint.NoneActive";
    public const string FrontMatterInvalid = "Backlog.FrontMatter.Invalid";
    public const string SprintIdConflict = "Backlog.Sprint.IdConflict";
    public const string SprintCommitInvalid = "Backlog.Sprint.CommitInvalid";
    public const string InputInvalid = "Backlog.Input.Invalid";
    public const string IoFailure = "Backlog.Io.Failure";
    public const string GitLookupFailed = "Backlog.Git.LookupFailed";
    public const string LintFailed = "Backlog.Lint.Failed";

    // Phase 2 - 4 additions
    public const string ItemNotReady = "Backlog.Item.NotReady";
    public const string ItemPointsInvalid = "Backlog.Item.PointsInvalid";
    public const string SprintCapacityExceeded = "Backlog.Sprint.CapacityExceeded";
    public const string SprintScopeInvalid = "Backlog.Sprint.ScopeInvalid";
    public const string TransitionNoCommits = "Backlog.Transition.NoCommits";
    public const string TransitionBuildFailed = "Backlog.Transition.BuildFailed";
}

internal static class BacklogConstants
{
    public static readonly FrozenSet<string> ValidStatuses =
        new[] { "icebox", "todo", "doing", "done", "dropped" }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static readonly FrozenSet<string> ValidPriorities =
        new[] { "P0", "P1", "P2", "P3" }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static readonly FrozenSet<string> ValidCategories =
        new[] { "task", "story", "feature", "debt", "spike" }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static readonly FrozenSet<int> ValidFibonacciPoints =
        new[] { 1, 2, 3, 5, 8, 13 }
            .ToFrozenSet();
}

internal sealed record ScopeChange
{
    [YamlMember(Alias = "date")]
    public required string Date { get; init; }

    [YamlMember(Alias = "id")]
    public required string Id { get; init; }

    [YamlMember(Alias = "action")]
    public required string Action { get; init; } // added | removed

    [YamlMember(Alias = "reason")]
    public required string Reason { get; init; }
}

internal sealed record BacklogRefs
{
    [YamlMember(Alias = "rules")]
    public List<string> Rules { get; init; } = [];

    [YamlMember(Alias = "docs")]
    public List<string> Docs { get; init; } = [];
}

internal sealed record BacklogFrontMatter
{
    [YamlMember(Alias = "id")]
    public required string Id { get; init; } // e.g. "#024"

    [YamlMember(Alias = "title")]
    public required string Title { get; init; }

    [YamlMember(Alias = "module")]
    public required string Module { get; init; } // Top-level parent pillar (Core, AI, etc.)

    [YamlMember(Alias = "category")]
    public string Category { get; init; } = "task"; // feature | story | task | debt | spike

    [YamlMember(Alias = "parent")]
    public string? Parent { get; init; } // Parent feature/task ID or RFC (e.g. "#010" or "RFC-002")

    [YamlMember(Alias = "legacy_id")]
    public string? LegacyId { get; init; } // Historical ID (e.g. "CORE-024")

    [YamlMember(Alias = "type")]
    public string Type { get; init; } = "debt"; // legacy type compatibility

    [YamlMember(Alias = "priority")]
    public string Priority { get; init; } = "P2"; // P0 | P1 | P2 | P3

    [YamlMember(Alias = "status")]
    public string Status { get; init; } = "todo"; // icebox | todo | doing | done | dropped

    [YamlMember(Alias = "sprint")]
    public string? Sprint { get; init; }

    [YamlMember(Alias = "points")]
    public int? Points { get; init; }

    [YamlMember(Alias = "rank")]
    public int? Rank { get; init; }

    [YamlMember(Alias = "started")]
    public string? Started { get; init; }

    [YamlMember(Alias = "carry_over_count")]
    public int CarryOverCount { get; init; } = 0;

    [YamlMember(Alias = "ready")]
    public bool Ready { get; init; } = false;

    [YamlMember(Alias = "estimate")]
    public int? Estimate { get; init; }

    [YamlMember(Alias = "created")]
    public string Created { get; init; } = string.Empty;

    [YamlMember(Alias = "closed")]
    public string? Closed { get; init; }

    [YamlMember(Alias = "refs")]
    public BacklogRefs Refs { get; init; } = new();

    [YamlMember(Alias = "commits")]
    public List<string> Commits { get; init; } = [];
}

internal sealed record BacklogItemModel
{
    public required BacklogFrontMatter FrontMatter { get; init; }
    public required string BodyMarkdown { get; init; }
    public required string FilePath { get; init; }
    public required bool IsArchived { get; init; }
}

internal sealed record SprintFrontMatter
{
    [YamlMember(Alias = "id")]
    public required string Id { get; init; }

    [YamlMember(Alias = "goal")]
    public required string Goal { get; init; }

    [YamlMember(Alias = "start")]
    public required string Start { get; init; }

    [YamlMember(Alias = "end")]
    public required string End { get; init; }

    [YamlMember(Alias = "status")]
    public string Status { get; init; } = "active"; // active | closed

    [YamlMember(Alias = "committed")]
    public List<string> Committed { get; init; } = [];

    [YamlMember(Alias = "committed_points")]
    public int? CommittedPoints { get; init; }

    [YamlMember(Alias = "capacity_points")]
    public int? CapacityPoints { get; init; }

    [YamlMember(Alias = "completed_points")]
    public int? CompletedPoints { get; init; }

    [YamlMember(Alias = "goal_met")]
    public bool? GoalMet { get; init; }

    [YamlMember(Alias = "scope_changes")]
    public List<ScopeChange> ScopeChanges { get; init; } = [];

    [YamlMember(Alias = "carried_over")]
    public List<string> CarriedOver { get; init; } = [];

    [YamlMember(Alias = "try_items")]
    public List<string> TryItems { get; init; } = [];
}
