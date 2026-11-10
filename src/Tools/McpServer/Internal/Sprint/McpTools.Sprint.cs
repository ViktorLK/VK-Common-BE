using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using VK.Tools.McpServer.Internal.Backlogs;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    // ==========================================
    // 1. Backlog Item Operations
    // ==========================================

    [McpServerTool] // [ARCH-07]
    [Description("Adds a new atomic task item with a global sequential ID (#NNN) to docs/07-Backlogs/{ParentModule}/.")]
    public async Task<string> VKBeAddBacklogItem(
        [Description("The module name or sub-slice (e.g., 'Core', 'AI.Cognitive', 'Persistence.EFCore'). Automatically maps to parent pillar.")] string module,
        [Description("A short, descriptive title for the task.")] string title,
        [Description("Detailed description of what needs to be done.")] string description,
        [Description("Priority level: 'P0', 'P1', 'P2', 'P3' (or legacy 'High', 'Medium', 'Low').")] string priority = "P2",
        [Description("Target code or component affected (optional).")] string? target = null,
        [Description("Reference to rules or documents (optional).")] string? reference = null,
        [Description("Task category: 'task', 'story', 'feature', 'debt', 'spike'.")] string category = "task",
        [Description("Parent task or RFC ID (optional, e.g., '#010' or 'RFC-002').")] string? parent = null,
        [Description("Initial status: 'todo' or 'icebox'.")] string status = "todo",
        [Description("Story Points (Fibonacci: 1, 2, 3, 5, 8, 13). Optional.")] int? points = null,
        [Description("Estimated effort in hours (optional legacy).")] int? estimate = null,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return $"[{McpBacklogErrors.InputInvalid}] Title cannot be empty.";
            }

            var normalizedPriority = priority.ToUpperInvariant().Trim() switch
            {
                "HIGH" or "P1" => "P1",
                "P0" or "URGENT" => "P0",
                "LOW" or "P3" => "P3",
                "MEDIUM" or "P2" => "P2",
                var p when BacklogConstants.ValidPriorities.Contains(p) => p,
                _ => null
            };

            if (normalizedPriority == null)
            {
                return $"[{McpBacklogErrors.InputInvalid}] Invalid priority '{priority}'. Allowed values: {string.Join(", ", BacklogConstants.ValidPriorities)}.";
            }

            var normalizedCategory = category.ToLowerInvariant().Trim();
            if (!BacklogConstants.ValidCategories.Contains(normalizedCategory))
            {
                return $"[{McpBacklogErrors.InputInvalid}] Invalid category '{category}'. Allowed values: {string.Join(", ", BacklogConstants.ValidCategories)}.";
            }

            var normalizedStatus = status.ToLowerInvariant().Trim();
            if (!BacklogConstants.ValidStatuses.Contains(normalizedStatus))
            {
                return $"[{McpBacklogErrors.InputInvalid}] Invalid status '{status}'. Allowed values: {string.Join(", ", BacklogConstants.ValidStatuses)}.";
            }

            if (points.HasValue && !BacklogConstants.ValidFibonacciPoints.Contains(points.Value))
            {
                return $"[{McpBacklogErrors.ItemPointsInvalid}] Invalid Story Points '{points.Value}'. Allowed Fibonacci values: {string.Join(", ", BacklogConstants.ValidFibonacciPoints)}.";
            }

            var projectRoot = FindProjectRoot();
            var parentModule = BacklogRepository.ResolvePillarModule(module);
            var backlogsDir = BacklogRepository.GetBacklogsDir(projectRoot);
            var moduleDir = Path.Combine(backlogsDir, parentModule);

            if (!Directory.Exists(moduleDir))
            {
                Directory.CreateDirectory(moduleDir);
            }

            var nextIndex = await BacklogRepository.GetNextGlobalIndexAsync(projectRoot, ct).ConfigureAwait(false);
            var taskId = $"#{nextIndex:D3}";
            var slug = BacklogRepository.NormalizeSlug(title);
            var fileName = $"pbi-{nextIndex:D3}-{slug}.md";
            var filePath = Path.Combine(moduleDir, fileName);

            var refs = new BacklogRefs();
            if (!string.IsNullOrWhiteSpace(reference))
            {
                var parts = reference.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var part in parts)
                {
                    if (part.StartsWith("/") || part.StartsWith("docs", StringComparison.OrdinalIgnoreCase))
                    {
                        refs.Docs.Add(part);
                    }
                    else
                    {
                        refs.Rules.Add(part);
                    }
                }
            }

            var nowStr = _timeProvider.GetLocalNow().ToString("yyyy-MM-dd");

            var formattedDescription = description.Contains("### 1. Problem / Context", StringComparison.OrdinalIgnoreCase)
                ? description
                : $"### 1. Problem / Context (現状の課題と背景)\n{description}\n\n### 2. Proposed Changes (具体的な改修方針)\n- **Target**: `{target ?? "N/A"}`\n- **Reference**: {reference ?? "N/A"}";

            var dodText = BacklogRepository.GenerateDoDForCategory(normalizedCategory, title);

            var body = $"""
# Task: {title}
**ID**: {taskId}
**Target**: `{target ?? "N/A"}`
**Ref**: {reference ?? "N/A"}

## 📝 Description
{formattedDescription}

{dodText}
""";

            var candidateFm = new BacklogFrontMatter
            {
                Id = taskId,
                Title = title,
                Module = parentModule,
                Category = normalizedCategory,
                Parent = parent,
                LegacyId = null,
                Type = normalizedCategory == "debt" ? "debt" : "feature",
                Priority = normalizedPriority,
                Status = normalizedStatus,
                Sprint = null,
                Points = points,
                Rank = null,
                Started = null,
                CarryOverCount = 0,
                Ready = false,
                Estimate = estimate,
                Created = nowStr,
                Closed = null,
                Refs = refs,
                Commits = []
            };

            var (isReady, _) = BacklogRepository.EvaluateDoR(candidateFm, body);
            var finalFm = candidateFm with { Ready = isReady };

            var fullContent = BacklogRepository.SerializeWithFrontMatter(finalFm, body);
            await File.WriteAllTextAsync(filePath, fullContent, ct).ConfigureAwait(false);

            await BoardGenerator.GenerateAllBoardsAsync(projectRoot, ct).ConfigureAwait(false);

            var readyBadge = isReady ? "✅ Ready" : "⏳ Not Ready";
            var pointsBadge = points.HasValue ? $"{points.Value} pts" : "Unestimated";
            return $"[MCP Tool: VKAddBacklogItem]\n✅ Created atomic task: {taskId} with YAML front-matter v2\n- **Parent Module**: docs/07-Backlogs/{parentModule}/{fileName}\n- **Priority**: {normalizedPriority} | **Category**: {normalizedCategory} | **Status**: {normalizedStatus}\n- **Estimation**: {pointsBadge} | **DoR Status**: {readyBadge}\n- **Boards**: Product_Backlog.md and Active_Sprint.md regenerated.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"[{McpBacklogErrors.IoFailure}] Failed to add backlog item: {ex.Message}";
        }
    }

    [McpServerTool] // [ARCH-07]
    [Description("Refines one or more backlog items (comma-separated IDs supported) by updating estimation points, rank, priority, and optionally validating DoR to mark Ready.")]
    public async Task<string> VKBeBacklogRefine(
        [Description("Task ID or comma-separated IDs (e.g. '#024' or '#047,#050').")] string id,
        [Description("Story Points (Fibonacci: 1, 2, 3, 5, 8, 13). Optional.")] int? points = null,
        [Description("Relative rank within priority (lower number = higher priority). Optional.")] int? rank = null,
        [Description("Updated priority level: 'P0', 'P1', 'P2', 'P3'. Optional.")] string? priority = null,
        [Description("Whether to evaluate DoR and mark as Ready (true/false). Optional.")] bool? markReady = null,
        CancellationToken ct = default)
    {
        try
        {
            var rawIds = id.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (rawIds.Count == 0)
            {
                return $"[{McpBacklogErrors.InputInvalid}] Task ID cannot be empty.";
            }

            if (points.HasValue && !BacklogConstants.ValidFibonacciPoints.Contains(points.Value))
            {
                return $"[{McpBacklogErrors.ItemPointsInvalid}] Invalid Story Points '{points.Value}'. Allowed Fibonacci values: {string.Join(", ", BacklogConstants.ValidFibonacciPoints)}.";
            }

            string? normalizedPriority = null;
            if (!string.IsNullOrWhiteSpace(priority))
            {
                var normP = priority.ToUpperInvariant().Trim();
                if (!BacklogConstants.ValidPriorities.Contains(normP))
                {
                    return $"[{McpBacklogErrors.InputInvalid}] Invalid priority '{priority}'. Allowed values: {string.Join(", ", BacklogConstants.ValidPriorities)}.";
                }
                normalizedPriority = normP;
            }

            var projectRoot = FindProjectRoot();
            var items = await BacklogRepository.LoadAllBacklogsAsync(projectRoot, ct).ConfigureAwait(false);
            var results = new List<string>();
            var modifiedAny = false;

            foreach (var singleId in rawIds)
            {
                var item = items.FirstOrDefault(i =>
                    string.Equals(i.FrontMatter.Id, singleId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(i.FrontMatter.LegacyId, singleId, StringComparison.OrdinalIgnoreCase));

                if (item == null)
                {
                    results.Add($"❌ '{singleId}': Not found");
                    continue;
                }

                var updatedPriority = normalizedPriority ?? item.FrontMatter.Priority;
                var updatedPoints = points ?? item.FrontMatter.Points;
                var updatedRank = rank ?? item.FrontMatter.Rank;

                var updatedFm = item.FrontMatter with
                {
                    Points = updatedPoints,
                    Rank = updatedRank,
                    Priority = updatedPriority
                };

                var (isReady, deficiencies) = BacklogRepository.EvaluateDoR(updatedFm, item.BodyMarkdown);

                if (markReady == true && !isReady)
                {
                    results.Add($"⚠️ '{singleId}': DoR not met ({string.Join("; ", deficiencies)})");
                    continue;
                }

                var finalReady = markReady ?? isReady;
                updatedFm = updatedFm with { Ready = finalReady };

                var fullContent = BacklogRepository.SerializeWithFrontMatter(updatedFm, item.BodyMarkdown);
                await File.WriteAllTextAsync(item.FilePath, fullContent, ct).ConfigureAwait(false);
                modifiedAny = true;

                var readyBadge = finalReady ? "✅ Ready" : "⏳ Not Ready";
                var ptsBadge = updatedPoints.HasValue ? $"{updatedPoints.Value} pts" : "-";
                results.Add($"✅ '{singleId}': {updatedPriority} | {ptsBadge} | {readyBadge}");
            }

            if (modifiedAny)
            {
                await BoardGenerator.GenerateAllBoardsAsync(projectRoot, ct).ConfigureAwait(false);
            }

            if (rawIds.Count == 1)
            {
                return $"[MCP Tool: VKBeBacklogRefine]\n{results[0]}\n- **Boards Regenerated**: Product_Backlog.md, Active_Sprint.md";
            }

            return $"[MCP Tool: VKBeBacklogRefine] Batch Refined {results.Count} items:\n- " + string.Join("\n- ", results) + "\n- **Boards Regenerated**: Product_Backlog.md, Active_Sprint.md";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"[{McpBacklogErrors.IoFailure}] Failed to refine backlog item: {ex.Message}";
        }
    }

    [McpServerTool] // [ARCH-07]
    [Description("Transitions the status of a backlog item in-place ('icebox', 'todo', 'doing', 'done', 'dropped') with zero git-mv.")]
    public async Task<string> VKBeBacklogTransition(
        [Description("Task ID (e.g. '#024' or legacy 'CORE-024').")] string id,
        [Description("Target status: 'icebox', 'todo', 'doing', 'done', 'dropped'.")] string toStatus,
        [Description("Optional note or reason (mandatory for 'dropped').")] string? note = null,
        [Description("Whether to permit completion with zero commits (e.g. for spikes/docs). Default is false.")] bool allowNoCommits = false,
        [Description("Whether to run 'dotnet build -warnaserror' before marking done. Default is false.")] bool verifyBuild = false,
        [Description("Whether to automatically check off all DoD check boxes if commits and build validation pass. Default is false.")] bool completeDod = false,
        [Description("Whether to allow transitioning to 'doing' even if outside active sprint. Default is false.")] bool allowOutsideSprint = false,
        [Description("Whether to allow exceeding the WIP limit (max 2 active doing tasks). Default is false.")] bool allowExceedWip = false,
        CancellationToken ct = default)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var items = await BacklogRepository.LoadAllBacklogsAsync(projectRoot, ct).ConfigureAwait(false);
            var item = items.FirstOrDefault(i =>
                string.Equals(i.FrontMatter.Id, id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(i.FrontMatter.LegacyId, id, StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                return $"[{McpBacklogErrors.ItemNotFound}] Backlog item '{id}' was not found in docs/07-Backlogs/.";
            }

            var currentStatus = item.FrontMatter.Status;
            var targetStatus = toStatus.ToLowerInvariant().Trim();

            if (!BacklogConstants.ValidStatuses.Contains(targetStatus))
            {
                return $"[{McpBacklogErrors.InputInvalid}] Invalid target status '{toStatus}'. Allowed values: {string.Join(", ", BacklogConstants.ValidStatuses)}.";
            }

            // Validate transition
            var allowed = (currentStatus, targetStatus) switch
            {
                ("icebox", "todo") => true,
                ("icebox", "dropped") => true,
                ("todo", "doing") => true,
                ("todo", "dropped") => true,
                ("doing", "todo") => true, // carry-over / cancel start
                ("doing", "done") => true,
                ("doing", "dropped") => true,
                _ when currentStatus == targetStatus => true,
                _ => false
            };

            if (!allowed)
            {
                return $"[{McpBacklogErrors.TransitionInvalid}] Cannot transition '{id}' from '{currentStatus}' to '{targetStatus}'.";
            }

            if (targetStatus == "dropped" && string.IsNullOrWhiteSpace(note))
            {
                return $"[{McpBacklogErrors.TransitionInvalid}] A note is required when dropping a task (explaining why it was abandoned or obsoleted).";
            }

            // Sprint Boundary & WIP Limit Check
            if (targetStatus == "doing" && currentStatus != "doing")
            {
                var activeSprint = await BoardGenerator.FindActiveSprintAsync(projectRoot, ct).ConfigureAwait(false);
                if (activeSprint.HasValue && !allowOutsideSprint)
                {
                    var sfm = activeSprint.Value.FrontMatter;
                    var isCommitted = sfm.Committed.Contains(item.FrontMatter.Id) ||
                        (item.FrontMatter.LegacyId != null && sfm.Committed.Contains(item.FrontMatter.LegacyId)) ||
                        item.FrontMatter.Sprint == sfm.Id;

                    if (!isCommitted)
                    {
                        return $"[{McpBacklogErrors.SprintScopeInvalid}] Cannot start task '{id}': It is not committed to active sprint '{sfm.Id}'. Use 'vk_be_sprint_scope(action: \"add\")' to include it first, or pass allowOutsideSprint=true.";
                    }
                }

                // WIP Limit check (default max 2 tasks in 'doing')
                var currentDoingCount = items.Count(i => i.FrontMatter.Status == "doing" && !string.Equals(i.FrontMatter.Id, item.FrontMatter.Id, StringComparison.OrdinalIgnoreCase));
                if (currentDoingCount >= 2 && !allowExceedWip)
                {
                    return $"[{McpBacklogErrors.TransitionInvalid}] WIP Limit exceeded: Already {currentDoingCount} task(s) currently in 'doing'. Complete or postpone ongoing work, or pass allowExceedWip=true to proceed.";
                }
            }

            var commits = new List<string>(item.FrontMatter.Commits);
            string? gitNotice = null;
            var body = item.BodyMarkdown;

            if (targetStatus == "done")
            {
                // Verify build if requested
                if (verifyBuild)
                {
                    var buildPsi = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "build --no-incremental -warnaserror",
                        WorkingDirectory = projectRoot,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using var buildProc = Process.Start(buildPsi);
                    if (buildProc != null)
                    {
                        await buildProc.WaitForExitAsync(ct).ConfigureAwait(false);
                        if (buildProc.ExitCode != 0)
                        {
                            return $"[{McpBacklogErrors.TransitionBuildFailed}] Cannot mark '{id}' as done: 'dotnet build -warnaserror' failed with exit code {buildProc.ExitCode}. Fix compiler errors or warnings first.";
                        }
                    }
                }

                // Collect commits from git
                var (gitCommits, gitError) = BacklogRepository.FindGitCommitsForTask(projectRoot, item.FrontMatter.Id);
                if (item.FrontMatter.LegacyId != null)
                {
                    var (legacyCommits, legacyGitError) = BacklogRepository.FindGitCommitsForTask(projectRoot, item.FrontMatter.LegacyId);
                    gitCommits.AddRange(legacyCommits);
                    gitError ??= legacyGitError;
                }

                if (gitError != null)
                {
                    gitNotice = $"\n⚠️ [{McpBacklogErrors.GitLookupFailed}] Git history lookup warning: {gitError}";
                }

                foreach (var c in gitCommits)
                {
                    if (!commits.Contains(c)) commits.Add(c);
                }

                if (commits.Count == 0 && !allowNoCommits)
                {
                    return $"[{McpBacklogErrors.TransitionNoCommits}] Cannot mark '{id}' as done: No linked git commits found. If this task produced no code commits (e.g. spike, doc), provide allowNoCommits=true with an explanatory note.";
                }

                // Complete DoD or verify
                if (completeDod)
                {
                    body = Regex.Replace(body, @"^-\s*\[\s*\]", "- [x]", RegexOptions.Multiline);
                }
                else
                {
                    var uncheckedBoxes = Regex.Matches(body, @"^-\s*\[\s*\]", RegexOptions.Multiline);
                    if (uncheckedBoxes.Count > 0)
                    {
                        return $"[{McpBacklogErrors.TransitionDodIncomplete}] Cannot mark '{id}' as done: There are {uncheckedBoxes.Count} unchecked DoD items in the file. Pass completeDod=true or complete them first.";
                    }
                }
            }

            var nowStr = _timeProvider.GetLocalNow().ToString("yyyy-MM-dd");
            var startedStr = (targetStatus == "doing" && string.IsNullOrEmpty(item.FrontMatter.Started))
                ? nowStr
                : item.FrontMatter.Started;

            var updatedFm = item.FrontMatter with
            {
                Status = targetStatus,
                Started = startedStr,
                Closed = (targetStatus == "done" || targetStatus == "dropped") ? nowStr : null,
                Commits = commits
            };

            if (!string.IsNullOrWhiteSpace(note))
            {
                body += $"\n\n> **Status Change Note ({nowStr})**: {note}\n";
            }

            // Zero git mv: strictly update the file in-place!
            var currentFilePath = item.FilePath;
            var fullContent = BacklogRepository.SerializeWithFrontMatter(updatedFm, body);
            await File.WriteAllTextAsync(currentFilePath, fullContent, ct).ConfigureAwait(false);

            await BoardGenerator.GenerateAllBoardsAsync(projectRoot, ct).ConfigureAwait(false);

            var gitSummary = gitNotice ?? string.Empty;
            return $"[MCP Tool: VKBeBacklogTransition]\n✅ Transitioned '{id}' in-place: {currentStatus} -> {targetStatus} (Zero git-mv)\n- **Path**: {Path.GetRelativePath(projectRoot, currentFilePath)}\n- **Commits Linked**: {commits.Count}{gitSummary}\n- **Boards Regenerated**: Product_Backlog.md, Active_Sprint.md";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"[{McpBacklogErrors.IoFailure}] Error during transition: {ex.Message}";
        }
    }

    [McpServerTool] // [ARCH-07]
    [Description("Lints all backlog items for front-matter schema validity, duplicate IDs, DoR conformance, and broken references.")]
    public async Task<string> VKBeBacklogLint(CancellationToken ct = default)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var backlogsDir = BacklogRepository.GetBacklogsDir(projectRoot);
            var sprintsDir = BacklogRepository.GetSprintsDir(projectRoot);

            var files = Directory.GetFiles(backlogsDir, "*.md", SearchOption.AllDirectories)
                .Where(f => !Path.GetFileName(f).Equals("Active_Sprint.md", StringComparison.OrdinalIgnoreCase) &&
                            !Path.GetFileName(f).Equals("Product_Backlog.md", StringComparison.OrdinalIgnoreCase) &&
                            !Path.GetFileName(f).Equals("Health_Scorecard.md", StringComparison.OrdinalIgnoreCase) &&
                            !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase) &&
                            !f.Contains(Path.Combine("07-Backlogs", "Sprints"), StringComparison.OrdinalIgnoreCase))
                .ToList();

            var issues = new List<string>();
            var seenIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                var text = await File.ReadAllTextAsync(file, ct).ConfigureAwait(false);
                var rel = Path.GetRelativePath(projectRoot, file);

                if (!BacklogRepository.TryParseFrontMatter<BacklogFrontMatter>(text, out var fm, out var body, out var parseError) || fm == null)
                {
                    issues.Add($"❌ [{McpBacklogErrors.FrontMatterInvalid}] {rel}: {parseError ?? "Unparseable front-matter"}");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(fm.Id))
                {
                    issues.Add($"❌ [{McpBacklogErrors.FrontMatterInvalid}] {rel}: Empty ID");
                }
                else
                {
                    if (seenIds.TryGetValue(fm.Id, out var existingPath))
                    {
                        issues.Add($"❌ [{McpBacklogErrors.ItemDuplicateId}] Duplicate ID '{fm.Id}': '{rel}' conflicts with '{existingPath}'");
                    }
                    else
                    {
                        seenIds[fm.Id] = rel;
                    }

                    // Check filename matches ID
                    var fileName = Path.GetFileName(file);
                    var cleanNum = Regex.Match(fm.Id, @"\d+");
                    if (cleanNum.Success && !fileName.Contains(cleanNum.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        issues.Add($"❌ [{McpBacklogErrors.FrontMatterInvalid}] {rel}: Filename does not contain ID number '{cleanNum.Value}'.");
                    }
                }

                if (!BacklogConstants.ValidStatuses.Contains(fm.Status))
                {
                    issues.Add($"❌ [{McpBacklogErrors.FrontMatterInvalid}] {rel}: Invalid status '{fm.Status}'. Allowed: {string.Join(", ", BacklogConstants.ValidStatuses)}");
                }

                if (!BacklogConstants.ValidPriorities.Contains(fm.Priority))
                {
                    issues.Add($"❌ [{McpBacklogErrors.FrontMatterInvalid}] {rel}: Invalid priority '{fm.Priority}'. Allowed: {string.Join(", ", BacklogConstants.ValidPriorities)}");
                }

                if (!BacklogConstants.ValidCategories.Contains(fm.Category))
                {
                    issues.Add($"❌ [{McpBacklogErrors.FrontMatterInvalid}] {rel}: Invalid category '{fm.Category}'. Allowed: {string.Join(", ", BacklogConstants.ValidCategories)}");
                }

                if (string.Equals(fm.Status, "done", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(fm.Closed))
                {
                    issues.Add($"❌ [{McpBacklogErrors.FrontMatterInvalid}] {rel}: Task marked done but 'closed' date is missing.");
                }

                if (string.Equals(fm.Status, "doing", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(fm.Started))
                    {
                        issues.Add($"❌ [{McpBacklogErrors.FrontMatterInvalid}] {rel}: Task is 'doing' but 'started' date is missing.");
                    }
                    if (string.IsNullOrWhiteSpace(fm.Sprint))
                    {
                        issues.Add($"⚠️ [{McpBacklogErrors.FrontMatterInvalid}] {rel}: Task is 'doing' but not assigned to any active Sprint.");
                    }
                }

                if (fm.Ready)
                {
                    var (isReady, defs) = BacklogRepository.EvaluateDoR(fm, body);
                    if (!isReady)
                    {
                        issues.Add($"❌ [{McpBacklogErrors.ItemNotReady}] {rel}: Marked ready: true but fails DoR ({string.Join("; ", defs)})");
                    }
                }

                if (fm.CarryOverCount >= 3)
                {
                    issues.Add($"⚠️ [Backlog.Stagnation] {rel}: Task has been carried over {fm.CarryOverCount} times. Consider decomposing or dropping.");
                }

                if (!string.IsNullOrWhiteSpace(fm.Sprint))
                {
                    var sprintFile = Path.Combine(sprintsDir, $"{fm.Sprint}.md");
                    if (!File.Exists(sprintFile))
                    {
                        issues.Add($"❌ [{McpBacklogErrors.FrontMatterInvalid}] {rel}: References non-existent sprint '{fm.Sprint}'.");
                    }
                }
            }

            if (issues.Count == 0)
            {
                return $"✅ [VKBeBacklogLint] All {files.Count} backlog items comply with front-matter standards. Zero defects.";
            }

            return $"🚩 [{McpBacklogErrors.LintFailed}] Found {issues.Count} issues across {files.Count} files:\n" + string.Join("\n", issues.Take(30));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"[{McpBacklogErrors.IoFailure}] Lint failed: {ex.Message}";
        }
    }

    // ==========================================
    // 2. Sprint Lifecycle Operations
    // ==========================================

    [McpServerTool] // [ARCH-07]
    [Description("Starts a new personal Sprint (default 7 days) with a goal and committed task IDs, creating docs/07-Backlogs/_sprints/S-{Year}-{Week}.md.")]
    public async Task<string> VKBeSprintStart(
        [Description("The overarching goal for this Sprint.")] string goal,
        [Description("Comma-separated list of backlog task IDs to commit (e.g. '#024,#023').")] string committedTaskIds,
        [Description("Sprint duration in days. Default is 7.")] int durationDays = 7,
        [Description("Explicitly allow committing points exceeding historical average velocity. Default is false.")] bool allowOverCapacity = false,
        [Description("Whether to overwrite an existing active sprint or sprint file for the current week. Default is false.")] bool overwrite = false,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(goal))
            {
                return $"[{McpBacklogErrors.InputInvalid}] Sprint goal cannot be empty.";
            }

            if (string.IsNullOrWhiteSpace(committedTaskIds))
            {
                return $"[{McpBacklogErrors.InputInvalid}] Committed task IDs cannot be empty.";
            }

            if (durationDays < 1 || durationDays > 28)
            {
                return $"[{McpBacklogErrors.InputInvalid}] Sprint durationDays must be between 1 and 28. Provided: {durationDays}.";
            }

            var projectRoot = FindProjectRoot();
            var activeSprint = await BoardGenerator.FindActiveSprintAsync(projectRoot, ct).ConfigureAwait(false);
            if (activeSprint != null && !overwrite)
            {
                return $"[{McpBacklogErrors.SprintAlreadyActive}] Sprint '{activeSprint.Value.FrontMatter.Id}' is currently active. Please close it first with vk_be_sprint_close or specify overwrite=true.";
            }

            // [CS.06] Deterministic time via TimeProvider + ISO calendar week calculation
            var now = _timeProvider.GetLocalNow();
            var isoYear = ISOWeek.GetYear(now.DateTime);
            var isoWeek = ISOWeek.GetWeekOfYear(now.DateTime);
            var sprintId = $"S-{isoYear}-{isoWeek:D2}";

            var startDateStr = now.ToString("yyyy-MM-dd");
            var endDateStr = now.AddDays(durationDays).ToString("yyyy-MM-dd");

            var ids = committedTaskIds
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var sprintsDir = BacklogRepository.GetSprintsDir(projectRoot);
            if (!Directory.Exists(sprintsDir)) Directory.CreateDirectory(sprintsDir);

            var sprintFilePath = Path.Combine(sprintsDir, $"{sprintId}.md");
            if (File.Exists(sprintFilePath) && !overwrite)
            {
                return $"[{McpBacklogErrors.SprintIdConflict}] Sprint file '{sprintId}.md' already exists. Specify overwrite=true to restart/overwrite it.";
            }

            // [AP.06] Verify all committed tasks before performing any mutation (no silent fallback/partial state)
            var allItems = await BacklogRepository.LoadAllBacklogsAsync(projectRoot, ct).ConfigureAwait(false);

            // Clean up old active sprint task references if overwrite is requested
            if (overwrite && activeSprint != null)
            {
                var oldSprintId = activeSprint.Value.FrontMatter.Id;
                var oldCommittedIds = activeSprint.Value.FrontMatter.Committed;
                var oldCommitted = allItems.Where(i =>
                    i.FrontMatter.Sprint == oldSprintId ||
                    oldCommittedIds.Contains(i.FrontMatter.Id) ||
                    (i.FrontMatter.LegacyId != null && oldCommittedIds.Contains(i.FrontMatter.LegacyId))).ToList();

                foreach (var oldTask in oldCommitted)
                {
                    if (!ids.Contains(oldTask.FrontMatter.Id, StringComparer.OrdinalIgnoreCase) &&
                        (oldTask.FrontMatter.LegacyId == null || !ids.Contains(oldTask.FrontMatter.LegacyId, StringComparer.OrdinalIgnoreCase)))
                    {
                        var clearedFm = oldTask.FrontMatter with { Sprint = null };
                        var clearedContent = BacklogRepository.SerializeWithFrontMatter(clearedFm, oldTask.BodyMarkdown);
                        await File.WriteAllTextAsync(oldTask.FilePath, clearedContent, ct).ConfigureAwait(false);
                    }
                }
            }
            var missingIds = new List<string>();
            var invalidStatusTasks = new List<(string Id, string Status)>();
            var notReadyTasks = new List<(string Id, string Reason)>();
            var targets = new List<BacklogItemModel>();

            foreach (var id in ids)
            {
                var target = allItems.FirstOrDefault(i =>
                    string.Equals(i.FrontMatter.Id, id, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(i.FrontMatter.LegacyId, id, StringComparison.OrdinalIgnoreCase));

                if (target == null)
                {
                    missingIds.Add(id);
                    continue;
                }

                if (!string.Equals(target.FrontMatter.Status, "todo", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(target.FrontMatter.Status, "doing", StringComparison.OrdinalIgnoreCase))
                {
                    invalidStatusTasks.Add((target.FrontMatter.Id, target.FrontMatter.Status));
                    continue;
                }

                // Check DoR
                if (!target.FrontMatter.Ready)
                {
                    var (isReady, defs) = BacklogRepository.EvaluateDoR(target.FrontMatter, target.BodyMarkdown);
                    if (!isReady)
                    {
                        notReadyTasks.Add((target.FrontMatter.Id, string.Join("; ", defs)));
                        continue;
                    }
                }

                targets.Add(target);
            }

            if (missingIds.Count > 0 || invalidStatusTasks.Count > 0 || notReadyTasks.Count > 0)
            {
                var errParts = new List<string>();
                if (missingIds.Count > 0)
                {
                    errParts.Add($"Non-existent tasks: {string.Join(", ", missingIds)}");
                }
                if (invalidStatusTasks.Count > 0)
                {
                    errParts.Add($"Tasks not in 'todo' or 'doing' status: {string.Join(", ", invalidStatusTasks.Select(x => $"{x.Id} ({x.Status})"))}");
                }
                if (notReadyTasks.Count > 0)
                {
                    errParts.Add($"Tasks failing DoR (not ready): {string.Join(", ", notReadyTasks.Select(x => $"{x.Id} [{x.Reason}]"))}");
                }
                return $"[{McpBacklogErrors.SprintCommitInvalid}] Cannot start sprint with invalid committed tasks:\n- {string.Join("\n- ", errParts)}\nRefine tasks with vk_be_backlog_refine first. No changes were made to any files.";
            }

            // Capacity Check
            var plannedPoints = targets.Sum(t => t.FrontMatter.Points ?? 0);
            var avgVelocity = await BacklogRepository.CalculateAverageVelocityAsync(projectRoot, ct).ConfigureAwait(false);
            if (avgVelocity.HasValue && plannedPoints > avgVelocity.Value && !allowOverCapacity)
            {
                return $"[{McpBacklogErrors.SprintCapacityExceeded}] Planned points ({plannedPoints} pts) exceeds historical capacity ({avgVelocity.Value} pts). To proceed anyway, specify allowOverCapacity=true.";
            }

            // Check previous sprint Try items follow-up
            var retroFollowUp = new List<string>();
            var allSprints = Directory.GetFiles(sprintsDir, "*.md");
            foreach (var sFile in allSprints.OrderByDescending(f => f))
            {
                var sText = await File.ReadAllTextAsync(sFile, ct).ConfigureAwait(false);
                if (BacklogRepository.TryParseFrontMatter<SprintFrontMatter>(sText, out var pastSfm, out _) && pastSfm != null)
                {
                    if (pastSfm.TryItems.Count > 0)
                    {
                        var unresolvedTry = allItems
                            .Where(i => pastSfm.TryItems.Contains(i.FrontMatter.Id))
                            .Where(i => i.FrontMatter.Status != "done" && i.FrontMatter.Status != "dropped")
                            .Select(i => $"{i.FrontMatter.Id} ({i.FrontMatter.Title})")
                            .ToList();
                        if (unresolvedTry.Count > 0)
                        {
                            retroFollowUp.AddRange(unresolvedTry);
                        }
                    }
                    break; // Check the latest closed sprint only
                }
            }

            var sprintFm = new SprintFrontMatter
            {
                Id = sprintId,
                Goal = goal,
                Start = startDateStr,
                End = endDateStr,
                Status = "active",
                Committed = targets.Select(t => t.FrontMatter.Id).ToList(),
                CommittedPoints = plannedPoints,
                CapacityPoints = avgVelocity,
                CompletedPoints = null,
                GoalMet = null,
                ScopeChanges = [],
                CarriedOver = [],
                TryItems = []
            };

            var body = $"""
# Sprint {sprintId}

## 🎯 Goal
{goal}

## 📋 Daily Log
- {startDateStr}: Sprint initialized with {targets.Count} committed tasks ({plannedPoints} pts). Capacity: {avgVelocity?.ToString() ?? "N/A"} pts.

## 🏁 Review & Retro
_Review and Retrospective will be recorded when closing the Sprint._
""";

            var sprintContent = BacklogRepository.SerializeWithFrontMatter(sprintFm, body);
            await File.WriteAllTextAsync(sprintFilePath, sprintContent, ct).ConfigureAwait(false);

            // Update committed tasks with sprint id
            foreach (var target in targets)
            {
                var updatedFm = target.FrontMatter with { Sprint = sprintId };
                var updatedContent = BacklogRepository.SerializeWithFrontMatter(updatedFm, target.BodyMarkdown);
                await File.WriteAllTextAsync(target.FilePath, updatedContent, ct).ConfigureAwait(false);
            }

            await BoardGenerator.GenerateAllBoardsAsync(projectRoot, ct).ConfigureAwait(false);

            var retroNotice = retroFollowUp.Count > 0
                ? $"\n- **Pending Retro Try Follow-Ups**: {string.Join(", ", retroFollowUp)}"
                : string.Empty;

            return $"[MCP Tool: VKBeSprintStart]\n🚀 Started Sprint: {sprintId}\n- **Goal**: {goal}\n- **Period**: {startDateStr} ~ {endDateStr} ({durationDays} days)\n- **Committed**: {targets.Count} tasks ({plannedPoints} pts)\n- **Capacity**: {avgVelocity?.ToString() ?? "N/A"} pts{retroNotice}\n- **File**: docs/07-Backlogs/_sprints/{sprintId}.md\n- **Active Board**: Regenerated Active_Sprint.md";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"[{McpBacklogErrors.IoFailure}] Failed to start sprint: {ex.Message}";
        }
    }

    [McpServerTool] // [ARCH-07]
    [Description("Manages scope changes (add/remove) within the current active Sprint with mandatory trace reason.")]
    public async Task<string> VKBeSprintScope(
        [Description("Task ID to add or remove (e.g. '#024').")] string id,
        [Description("Action: 'add' or 'remove'.")] string action,
        [Description("Mandatory reason for this scope change.")] string reason,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return $"[{McpBacklogErrors.InputInvalid}] A clear reason is required for any sprint scope change.";
            }

            var normalizedAction = action.ToLowerInvariant().Trim();
            if (normalizedAction != "add" && normalizedAction != "remove")
            {
                return $"[{McpBacklogErrors.InputInvalid}] Invalid action '{action}'. Allowed: 'add' or 'remove'.";
            }

            var projectRoot = FindProjectRoot();
            var activeSprint = await BoardGenerator.FindActiveSprintAsync(projectRoot, ct).ConfigureAwait(false);
            if (!activeSprint.HasValue)
            {
                return $"[{McpBacklogErrors.SprintNoneActive}] No active sprint is currently running.";
            }

            var (sfm, sprintPath) = activeSprint.Value;
            var items = await BacklogRepository.LoadAllBacklogsAsync(projectRoot, ct).ConfigureAwait(false);
            var item = items.FirstOrDefault(i =>
                string.Equals(i.FrontMatter.Id, id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(i.FrontMatter.LegacyId, id, StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                return $"[{McpBacklogErrors.ItemNotFound}] Task '{id}' was not found.";
            }

            var nowStr = _timeProvider.GetLocalNow().ToString("yyyy-MM-dd");
            var itemPoints = item.FrontMatter.Points ?? 0;

            if (normalizedAction == "add")
            {
                if (sfm.Committed.Contains(item.FrontMatter.Id) || item.FrontMatter.Sprint == sfm.Id)
                {
                    return $"[{McpBacklogErrors.SprintScopeInvalid}] Task '{id}' is already committed to active sprint '{sfm.Id}'.";
                }

                if (item.FrontMatter.Status != "todo" && item.FrontMatter.Status != "doing")
                {
                    return $"[{McpBacklogErrors.SprintScopeInvalid}] Cannot add task '{id}' with status '{item.FrontMatter.Status}'. Must be 'todo' or 'doing'.";
                }

                // Check DoR
                if (!item.FrontMatter.Ready)
                {
                    var (isReady, defs) = BacklogRepository.EvaluateDoR(item.FrontMatter, item.BodyMarkdown);
                    if (!isReady)
                    {
                        return $"[{McpBacklogErrors.ItemNotReady}] Cannot add task '{id}' to sprint: DoR not met ({string.Join("; ", defs)}).";
                    }
                }

                var updatedItemFm = item.FrontMatter with { Sprint = sfm.Id };
                var updatedItemContent = BacklogRepository.SerializeWithFrontMatter(updatedItemFm, item.BodyMarkdown);
                await File.WriteAllTextAsync(item.FilePath, updatedItemContent, ct).ConfigureAwait(false);

                var scopeChanges = new List<ScopeChange>(sfm.ScopeChanges)
                {
                    new() { Date = nowStr, Id = item.FrontMatter.Id, Action = "added", Reason = reason }
                };

                var updatedCommitted = new List<string>(sfm.Committed);
                if (!updatedCommitted.Contains(item.FrontMatter.Id)) updatedCommitted.Add(item.FrontMatter.Id);

                var sprintTasks = items.Where(i =>
                    updatedCommitted.Contains(i.FrontMatter.Id) ||
                    (i.FrontMatter.LegacyId != null && updatedCommitted.Contains(i.FrontMatter.LegacyId)) ||
                    string.Equals(i.FrontMatter.Id, item.FrontMatter.Id, StringComparison.OrdinalIgnoreCase)).ToList();
                var newCommittedPoints = sprintTasks.Sum(t => t.FrontMatter.Points ?? 0);

                var updatedSfm = sfm with
                {
                    Committed = updatedCommitted,
                    CommittedPoints = newCommittedPoints,
                    ScopeChanges = scopeChanges
                };

                var sprintText = await File.ReadAllTextAsync(sprintPath, ct).ConfigureAwait(false);
                BacklogRepository.TryParseFrontMatter<SprintFrontMatter>(sprintText, out _, out var sprintBody);
                var updatedSprintContent = BacklogRepository.SerializeWithFrontMatter(updatedSfm, sprintBody);
                await File.WriteAllTextAsync(sprintPath, updatedSprintContent, ct).ConfigureAwait(false);

                await BoardGenerator.GenerateAllBoardsAsync(projectRoot, ct).ConfigureAwait(false);

                return $"[MCP Tool: VKBeSprintScope]\n📋 Scope {normalizedAction} executed for '{id}':\n- **Reason**: {reason}\n- **Updated Points**: {updatedSfm.CommittedPoints} pts\n- **Boards Regenerated**: Active_Sprint.md";
            }
            else // remove
            {
                if (item.FrontMatter.Sprint != sfm.Id && !sfm.Committed.Contains(item.FrontMatter.Id) && (item.FrontMatter.LegacyId == null || !sfm.Committed.Contains(item.FrontMatter.LegacyId)))
                {
                    return $"[{McpBacklogErrors.SprintScopeInvalid}] Task '{id}' is not part of active sprint '{sfm.Id}'.";
                }

                var updatedItemFm = item.FrontMatter with { Sprint = null, Status = "todo" };
                var updatedItemContent = BacklogRepository.SerializeWithFrontMatter(updatedItemFm, item.BodyMarkdown);
                await File.WriteAllTextAsync(item.FilePath, updatedItemContent, ct).ConfigureAwait(false);

                var scopeChanges = new List<ScopeChange>(sfm.ScopeChanges)
                {
                    new() { Date = nowStr, Id = item.FrontMatter.Id, Action = "removed", Reason = reason }
                };

                var updatedCommitted = sfm.Committed.Where(c =>
                    !string.Equals(c, item.FrontMatter.Id, StringComparison.OrdinalIgnoreCase) &&
                    (item.FrontMatter.LegacyId == null || !string.Equals(c, item.FrontMatter.LegacyId, StringComparison.OrdinalIgnoreCase))).ToList();

                var sprintTasks = items.Where(i =>
                    !string.Equals(i.FrontMatter.Id, item.FrontMatter.Id, StringComparison.OrdinalIgnoreCase) &&
                    (updatedCommitted.Contains(i.FrontMatter.Id) || (i.FrontMatter.LegacyId != null && updatedCommitted.Contains(i.FrontMatter.LegacyId)))).ToList();
                var newCommittedPoints = sprintTasks.Sum(t => t.FrontMatter.Points ?? 0);

                var updatedSfm = sfm with
                {
                    Committed = updatedCommitted,
                    CommittedPoints = newCommittedPoints,
                    ScopeChanges = scopeChanges
                };

                var sprintText = await File.ReadAllTextAsync(sprintPath, ct).ConfigureAwait(false);
                BacklogRepository.TryParseFrontMatter<SprintFrontMatter>(sprintText, out _, out var sprintBody);
                var updatedSprintContent = BacklogRepository.SerializeWithFrontMatter(updatedSfm, sprintBody);
                await File.WriteAllTextAsync(sprintPath, updatedSprintContent, ct).ConfigureAwait(false);

                await BoardGenerator.GenerateAllBoardsAsync(projectRoot, ct).ConfigureAwait(false);

                return $"[MCP Tool: VKBeSprintScope]\n📋 Scope {normalizedAction} executed for '{id}':\n- **Reason**: {reason}\n- **Updated Points**: {updatedSfm.CommittedPoints} pts\n- **Boards Regenerated**: Active_Sprint.md";
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"[{McpBacklogErrors.IoFailure}] Failed to modify sprint scope: {ex.Message}";
        }
    }

    [McpServerTool] // [ARCH-07]
    [Description("Closes the current active Sprint, evaluating Goal achievement, carrying over incomplete tasks, and creating Try PBIs.")]
    public async Task<string> VKBeSprintClose(
        [Description("Mandatory evaluation of whether the Sprint Goal was achieved (true/false).")] bool? goalMet,
        [Description("Optional Retrospective notes (e.g. Keep, Problem, Try).")] string? retroNotes = null,
        [Description("Actionable Try items to automatically convert into icebox backlog items (semicolon or newline separated).")] string? tryItems = null,
        [Description("Target module to file Retro Try PBIs under (optional, defaults to dominant module in sprint or 'Core').")] string? tryModule = null,
        CancellationToken ct = default)
    {
        try
        {
            if (!goalMet.HasValue)
            {
                return $"[{McpBacklogErrors.InputInvalid}] 'goalMet' (true/false) is mandatory when closing a Sprint to track goal achievement rate.";
            }

            var projectRoot = FindProjectRoot();
            var activeSprint = await BoardGenerator.FindActiveSprintAsync(projectRoot, ct).ConfigureAwait(false);
            if (activeSprint == null)
            {
                return $"[{McpBacklogErrors.SprintNoneActive}] No active sprint is currently running.";
            }

            var (sfm, sprintPath) = activeSprint.Value;
            var items = await BacklogRepository.LoadAllBacklogsAsync(projectRoot, ct).ConfigureAwait(false);
            var sprintItems = items.Where(i =>
                i.FrontMatter.Sprint == sfm.Id ||
                sfm.Committed.Contains(i.FrontMatter.Id) ||
                (i.FrontMatter.LegacyId != null && sfm.Committed.Contains(i.FrontMatter.LegacyId))).ToList();

            var doneItems = sprintItems.Where(i => i.FrontMatter.Status == "done").ToList();
            var carryOverItems = sprintItems.Where(i => i.FrontMatter.Status != "done" && i.FrontMatter.Status != "dropped").ToList();

            // Carry over incomplete tasks: increment carry_over_count, clear sprint, reset to todo
            foreach (var item in carryOverItems)
            {
                var updatedFm = item.FrontMatter with
                {
                    Sprint = null,
                    Status = "todo",
                    CarryOverCount = item.FrontMatter.CarryOverCount + 1
                };
                var content = BacklogRepository.SerializeWithFrontMatter(updatedFm, item.BodyMarkdown);
                await File.WriteAllTextAsync(item.FilePath, content, ct).ConfigureAwait(false);
            }

            var nowStr = _timeProvider.GetLocalNow().ToString("yyyy-MM-dd");
            var completedPoints = doneItems.Sum(d => d.FrontMatter.Points ?? 0);

            // Automatically resolve target module for retro Try PBIs
            var targetModule = !string.IsNullOrWhiteSpace(tryModule)
                ? BacklogRepository.ResolvePillarModule(tryModule)
                : (sprintItems.GroupBy(i => i.FrontMatter.Module).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "Core");

            // Automatically create icebox PBIs from Try items
            var createdTryPbis = new List<string>();
            if (!string.IsNullOrWhiteSpace(tryItems))
            {
                var rawLines = tryItems.Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var line in rawLines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cleanTitle = line.TrimStart('-', '*', ' ');
                    var pbiResult = await VKBeAddBacklogItem(
                        module: targetModule,
                        title: $"[Retro Try] {cleanTitle}",
                        description: $"Generated automatically from Sprint {sfm.Id} Retrospective Try action item:\n{cleanTitle}",
                        category: "task",
                        status: "icebox",
                        ct: ct).ConfigureAwait(false);

                    var idMatch = Regex.Match(pbiResult, @"#\d+");
                    if (idMatch.Success) createdTryPbis.Add(idMatch.Value);
                }
            }

            var closedSfm = sfm with
            {
                Status = "closed",
                GoalMet = goalMet.Value,
                CompletedPoints = completedPoints,
                CarriedOver = carryOverItems.Select(c => c.FrontMatter.Id).ToList(),
                TryItems = createdTryPbis
            };

            var text = await File.ReadAllTextAsync(sprintPath, ct).ConfigureAwait(false);
            BacklogRepository.TryParseFrontMatter<SprintFrontMatter>(text, out _, out var body);

            var reviewSb = new StringBuilder();
            reviewSb.AppendLine($"\n\n## 🏁 Sprint Review ({nowStr})");
            reviewSb.AppendLine($"- **Sprint Goal Met**: {(goalMet.Value ? "✅ Yes" : "❌ No")}");
            reviewSb.AppendLine($"- **Completed**: {doneItems.Count} / {sprintItems.Count} tasks ({completedPoints} pts)");
            reviewSb.AppendLine($"- **Carried Over**: {carryOverItems.Count} tasks");
            reviewSb.AppendLine();
            reviewSb.AppendLine("### ✅ Done Items");
            if (doneItems.Count == 0)
            {
                reviewSb.AppendLine("- (None)");
            }
            else
            {
                foreach (var d in doneItems)
                {
                    var pts = d.FrontMatter.Points.HasValue ? $" ({d.FrontMatter.Points} pts)" : "";
                    reviewSb.AppendLine($"- [{d.FrontMatter.Id}] {d.FrontMatter.Title} ({d.FrontMatter.Priority}{pts})");
                }
            }

            // [Phase 3.3] Auto-generate sprint Changelog / Increments from task commits
            reviewSb.AppendLine();
            reviewSb.AppendLine("### 📦 Increment & Changelog");
            var itemsWithCommits = doneItems.Where(d => d.FrontMatter.Commits.Count > 0).ToList();
            if (itemsWithCommits.Count == 0)
            {
                reviewSb.AppendLine("- No commit hashes recorded for finished tasks.");
            }
            else
            {
                foreach (var d in itemsWithCommits)
                {
                    reviewSb.AppendLine($"- **{d.FrontMatter.Id}**: {d.FrontMatter.Title}");
                    foreach (var commit in d.FrontMatter.Commits)
                    {
                        reviewSb.AppendLine($"  - commit: `{commit}`");
                    }
                }
            }

            if (carryOverItems.Count > 0)
            {
                reviewSb.AppendLine();
                reviewSb.AppendLine("### ⏩ Carry-Over Items (Returned to Product Backlog)");
                foreach (var c in carryOverItems)
                {
                    reviewSb.AppendLine($"- [{c.FrontMatter.Id}] {c.FrontMatter.Title} (Carry-over count: {c.FrontMatter.CarryOverCount + 1})");
                }
            }

            reviewSb.AppendLine();
            reviewSb.AppendLine("### 💡 Retrospective (KPT)");
            if (!string.IsNullOrWhiteSpace(retroNotes))
            {
                reviewSb.AppendLine(retroNotes);
            }
            else
            {
                reviewSb.AppendLine("- **Keep**: 良かった点・継続したいこと");
                reviewSb.AppendLine("- **Problem**: 困ったこと・ボトルネック");
                reviewSb.AppendLine("- **Try**: 次の Sprint で試す改善アクション");
            }

            if (createdTryPbis.Count > 0)
            {
                reviewSb.AppendLine();
                reviewSb.AppendLine("### 📌 Action Items Transferred to Backlog");
                foreach (var tryId in createdTryPbis)
                {
                    reviewSb.AppendLine($"- Created icebox PBI: {tryId}");
                }
            }

            var updatedSprintContent = BacklogRepository.SerializeWithFrontMatter(closedSfm, body + reviewSb.ToString());
            await File.WriteAllTextAsync(sprintPath, updatedSprintContent, ct).ConfigureAwait(false);

            await BoardGenerator.GenerateAllBoardsAsync(projectRoot, ct).ConfigureAwait(false);

            return $"[MCP Tool: VKBeSprintClose]\n🏁 Closed Sprint: {sfm.Id}\n- **Goal Met**: {(goalMet.Value ? "✅ Yes" : "❌ No")}\n- **Velocity**: {completedPoints} pts ({doneItems.Count} tasks)\n- **Carry Over**: {carryOverItems.Count} tasks\n- **Try Items Created**: {createdTryPbis.Count}\n- **Archive**: docs/07-Backlogs/_sprints/{Path.GetFileName(sprintPath)}\n- **Active Board**: Active_Sprint.md updated.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"[{McpBacklogErrors.IoFailure}] Failed to close sprint: {ex.Message}";
        }
    }

    [McpServerTool] // [ARCH-07]
    [Description("Resets/cancels the current active Sprint, clearing Sprint assignments on all committed tasks and deleting or dropping the sprint document.")]
    public async Task<string> VKBeSprintReset(
        [Description("Whether to permanently delete the active sprint file instead of marking it dropped. Default is true.")] bool deleteFile = true,
        CancellationToken ct = default)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var activeSprint = await BoardGenerator.FindActiveSprintAsync(projectRoot, ct).ConfigureAwait(false);
            if (!activeSprint.HasValue)
            {
                return $"[{McpBacklogErrors.SprintNoneActive}] No active sprint is currently running to reset.";
            }

            var (sfm, sprintPath) = activeSprint.Value;
            var items = await BacklogRepository.LoadAllBacklogsAsync(projectRoot, ct).ConfigureAwait(false);
            var sprintItems = items.Where(i =>
                i.FrontMatter.Sprint == sfm.Id ||
                sfm.Committed.Contains(i.FrontMatter.Id) ||
                (i.FrontMatter.LegacyId != null && sfm.Committed.Contains(i.FrontMatter.LegacyId))).ToList();

            foreach (var item in sprintItems)
            {
                var newStatus = item.FrontMatter.Status == "doing" ? "todo" : item.FrontMatter.Status;
                var updatedFm = item.FrontMatter with
                {
                    Sprint = null,
                    Status = newStatus
                };
                var content = BacklogRepository.SerializeWithFrontMatter(updatedFm, item.BodyMarkdown);
                await File.WriteAllTextAsync(item.FilePath, content, ct).ConfigureAwait(false);
            }

            if (deleteFile)
            {
                if (File.Exists(sprintPath))
                {
                    File.Delete(sprintPath);
                }
            }
            else
            {
                var droppedFm = sfm with { Status = "dropped" };
                var text = await File.ReadAllTextAsync(sprintPath, ct).ConfigureAwait(false);
                BacklogRepository.TryParseFrontMatter<SprintFrontMatter>(text, out _, out var body);
                var content = BacklogRepository.SerializeWithFrontMatter(droppedFm, body);
                await File.WriteAllTextAsync(sprintPath, content, ct).ConfigureAwait(false);
            }

            await BoardGenerator.GenerateAllBoardsAsync(projectRoot, ct).ConfigureAwait(false);

            var actionStr = deleteFile ? "deleted" : "marked dropped";
            return $"[MCP Tool: VKBeSprintReset]\n🔄 Successfully reset Sprint '{sfm.Id}' ({actionStr}).\n- Cleared Sprint assignment from {sprintItems.Count} tasks.\n- Boards regenerated.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"[{McpBacklogErrors.IoFailure}] Failed to reset sprint: {ex.Message}";
        }
    }

    [McpServerTool] // [ARCH-07]
    [Description("Appends a structured daily standup log (yesterday, today, blockers) to the active sprint document.")]
    public async Task<string> VKBeSprintDaily(
        [Description("What was accomplished yesterday.")] string yesterday,
        [Description("What will be worked on today.")] string today,
        [Description("Any obstacles, blockers, or dependencies (optional).")] string? blockers = null,
        CancellationToken ct = default)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var activeSprint = await BoardGenerator.FindActiveSprintAsync(projectRoot, ct).ConfigureAwait(false);
            if (activeSprint == null)
            {
                return $"[{McpBacklogErrors.SprintNoneActive}] No active sprint is currently running.";
            }

            var (sfm, sprintPath) = activeSprint.Value;
            var text = await File.ReadAllTextAsync(sprintPath, ct).ConfigureAwait(false);
            if (!BacklogRepository.TryParseFrontMatter<SprintFrontMatter>(text, out var fm, out var body) || fm == null)
            {
                return $"[{McpBacklogErrors.FrontMatterInvalid}] Could not parse front-matter of active sprint.";
            }

            var nowStr = _timeProvider.GetLocalNow().ToString("yyyy-MM-dd");
            var sb = new StringBuilder();
            sb.AppendLine($"- {nowStr} (Daily Standup):");
            sb.AppendLine($"  - **Yesterday**: {yesterday}");
            sb.AppendLine($"  - **Today**: {today}");
            if (!string.IsNullOrWhiteSpace(blockers))
            {
                sb.AppendLine($"  - 🚧 **Blocker**: {blockers}");
            }

            var logHeader = "## 📋 Daily Log";
            var headerIdx = body.IndexOf(logHeader, StringComparison.OrdinalIgnoreCase);
            if (headerIdx != -1)
            {
                var nextSectionIdx = body.IndexOf("## ", headerIdx + logHeader.Length, StringComparison.OrdinalIgnoreCase);
                if (nextSectionIdx != -1)
                {
                    body = body.Insert(nextSectionIdx, sb.ToString() + "\n");
                }
                else
                {
                    body += "\n" + sb.ToString();
                }
            }
            else
            {
                body += $"\n\n{logHeader}\n{sb}\n";
            }

            var updatedContent = BacklogRepository.SerializeWithFrontMatter(fm, body);
            await File.WriteAllTextAsync(sprintPath, updatedContent, ct).ConfigureAwait(false);

            await BoardGenerator.GenerateAllBoardsAsync(projectRoot, ct).ConfigureAwait(false);

            return $"[MCP Tool: VKBeSprintDaily] Recorded daily standup to Sprint {sfm.Id}. Active_Sprint.md regenerated.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"[{McpBacklogErrors.IoFailure}] Failed to log daily standup: {ex.Message}";
        }
    }

    [McpServerTool] // [ARCH-07]
    [Description("Appends a single daily log line to the active sprint document.")]
    public async Task<string> VKBeSprintLog(
        [Description("The log message to record.")] string message,
        CancellationToken ct = default)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var activeSprint = await BoardGenerator.FindActiveSprintAsync(projectRoot, ct).ConfigureAwait(false);
            if (activeSprint == null)
            {
                return $"[{McpBacklogErrors.SprintNoneActive}] No active sprint is currently running.";
            }

            var (sfm, sprintPath) = activeSprint.Value;
            var text = await File.ReadAllTextAsync(sprintPath, ct).ConfigureAwait(false);
            if (!BacklogRepository.TryParseFrontMatter<SprintFrontMatter>(text, out var fm, out var body) || fm == null)
            {
                return $"[{McpBacklogErrors.FrontMatterInvalid}] Could not parse front-matter of active sprint.";
            }

            var nowStr = _timeProvider.GetLocalNow().ToString("yyyy-MM-dd");
            var logLine = $"- {nowStr}: {message}";

            var logHeader = "## 📋 Daily Log";
            var headerIdx = body.IndexOf(logHeader, StringComparison.OrdinalIgnoreCase);
            if (headerIdx != -1)
            {
                var nextSectionIdx = body.IndexOf("## ", headerIdx + logHeader.Length, StringComparison.OrdinalIgnoreCase);
                if (nextSectionIdx != -1)
                {
                    body = body.Insert(nextSectionIdx, logLine + "\n");
                }
                else
                {
                    body += "\n" + logLine;
                }
            }
            else
            {
                body += $"\n\n{logHeader}\n{logLine}\n";
            }

            var updatedContent = BacklogRepository.SerializeWithFrontMatter(fm, body);
            await File.WriteAllTextAsync(sprintPath, updatedContent, ct).ConfigureAwait(false);

            return $"[MCP Tool: VKBeSprintLog] Logged to Sprint {sfm.Id}: {logLine}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"[{McpBacklogErrors.IoFailure}] Failed to log to sprint: {ex.Message}";
        }
    }

    [McpServerTool] // [ARCH-07]
    [Description("Gets real-time summary of the active sprint (remaining days, committed/done/doing/todo points, blockers, pace) as JSON.")]
    public async Task<string> VKBeSprintStatus(CancellationToken ct = default)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var activeSprint = await BoardGenerator.FindActiveSprintAsync(projectRoot, ct).ConfigureAwait(false);
            if (activeSprint == null)
            {
                return JsonSerializer.Serialize(new
                {
                    active = false,
                    message = "No active sprint is currently running."
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            var (sfm, sprintPath) = activeSprint.Value;
            var items = await BacklogRepository.LoadAllBacklogsAsync(projectRoot, ct).ConfigureAwait(false);
            var sprintItems = items.Where(i =>
                i.FrontMatter.Sprint == sfm.Id ||
                sfm.Committed.Contains(i.FrontMatter.Id) ||
                (i.FrontMatter.LegacyId != null && sfm.Committed.Contains(i.FrontMatter.LegacyId))).ToList();

            var todo = sprintItems.Where(i => i.FrontMatter.Status == "todo" || i.FrontMatter.Status == "icebox").ToList();
            var doing = sprintItems.Where(i => i.FrontMatter.Status == "doing").ToList();
            var done = sprintItems.Where(i => i.FrontMatter.Status == "done").ToList();

            var committedPts = sfm.CommittedPoints ?? sprintItems.Sum(i => i.FrontMatter.Points ?? 0);
            var donePoints = done.Sum(i => i.FrontMatter.Points ?? 0);
            var doingPoints = doing.Sum(i => i.FrontMatter.Points ?? 0);
            var todoPoints = todo.Sum(i => i.FrontMatter.Points ?? 0);

            var hasStart = DateOnly.TryParse(sfm.Start, out var startDate);
            var hasEnd = DateOnly.TryParse(sfm.End, out var endDate);
            var todayDate = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
            int totalDays = (hasStart && hasEnd) ? Math.Max(1, endDate.DayNumber - startDate.DayNumber) : 7;
            int currentDay = hasStart ? Math.Clamp(todayDate.DayNumber - startDate.DayNumber + 1, 1, totalDays + 1) : 1;
            int remainingDays = hasEnd ? Math.Max(0, endDate.DayNumber - todayDate.DayNumber) : 0;

            var totalTrackedPoints = Math.Max(committedPts, donePoints + doingPoints + todoPoints);
            var doneRate = totalTrackedPoints > 0 ? (donePoints / (double)totalTrackedPoints * 100) : 0;

            string paceStatus = "On Track";
            if (totalTrackedPoints > 0 && totalDays > 0)
            {
                double timeElapsedRate = Math.Clamp(currentDay / (double)totalDays * 100, 0, 100);
                if (doneRate < timeElapsedRate - 30) paceStatus = "Behind Pace";
                else if (doneRate < timeElapsedRate - 15) paceStatus = "At Risk";
            }

            // Check blockers from sprint text
            var text = await File.ReadAllTextAsync(sprintPath, ct).ConfigureAwait(false);
            var blockerLines = new List<string>();
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("- Blockers:", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("- Blocker:", StringComparison.OrdinalIgnoreCase))
                {
                    var msg = trimmed.Substring(trimmed.IndexOf(':') + 1).Trim();
                    if (!string.IsNullOrWhiteSpace(msg) &&
                        !string.Equals(msg, "なし", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(msg, "None", StringComparison.OrdinalIgnoreCase))
                    {
                        blockerLines.Add(msg);
                    }
                }
            }

            var statusDto = new
            {
                active = true,
                sprintId = sfm.Id,
                goal = sfm.Goal,
                start = sfm.Start,
                end = sfm.End,
                currentDay,
                totalDays,
                remainingDays,
                pace = paceStatus,
                progress = new
                {
                    donePoints,
                    doingPoints,
                    todoPoints,
                    totalTrackedPoints,
                    doneRatePercent = Math.Round(doneRate, 1)
                },
                tasks = new
                {
                    doneCount = done.Count,
                    doingCount = doing.Count,
                    todoCount = todo.Count,
                    doingTasks = doing.Select(x => new { id = x.FrontMatter.Id, title = x.FrontMatter.Title, points = x.FrontMatter.Points }),
                    todoTasks = todo.Select(x => new { id = x.FrontMatter.Id, title = x.FrontMatter.Title, points = x.FrontMatter.Points })
                },
                blockers = blockerLines
            };

            return JsonSerializer.Serialize(statusDto, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"[{McpBacklogErrors.IoFailure}] Failed to retrieve sprint status: {ex.Message}";
        }
    }

    [McpServerTool] // [ARCH-07]
    [Description("Explicitly regenerates Product_Backlog.md, Active_Sprint.md, and README.md from front-matter.")]
    public async Task<string> VKBeGenerateBoards(CancellationToken ct = default)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            await BoardGenerator.GenerateAllBoardsAsync(projectRoot, ct).ConfigureAwait(false);
            return "✅ Successfully regenerated Product_Backlog.md, Active_Sprint.md, and README.md from YAML front-matter.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"[{McpBacklogErrors.IoFailure}] Failed to generate boards: {ex.Message}";
        }
    }
}
