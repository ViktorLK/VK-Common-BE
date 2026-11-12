using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using VK.Tools.McpServer.Internal.Backlogs;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    [McpServerTool] // [ARCH-07]
    [Description("Scans the codebase and backlogs to generate a project health scorecard and architectural compliance dashboard.")]
    public async Task<string> VKBeGenerateDashboard(CancellationToken ct = default)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var backlogsDir = BacklogRepository.GetBacklogsDir(projectRoot);
            var sprintsDir = BacklogRepository.GetSprintsDir(projectRoot);
            var adrDir = Path.Combine(projectRoot, "docs", "04-ArchitectureDecisionRecords");
            var srcDir = Path.Combine(projectRoot, "src", "BuildingBlocks");

            // 1. Backlog Analysis via Repository
            var allItems = await BacklogRepository.LoadAllBacklogsAsync(projectRoot, ct).ConfigureAwait(false);
            var activeItems = allItems.Where(i => !i.IsArchived && i.FrontMatter.Status != "done" && i.FrontMatter.Status != "dropped").ToList();

            int p0Count = activeItems.Count(i => i.FrontMatter.Priority == "P0");
            int p1Count = activeItems.Count(i => i.FrontMatter.Priority == "P1");
            int p2Count = activeItems.Count(i => i.FrontMatter.Priority == "P2");
            int p3Count = activeItems.Count(i => i.FrontMatter.Priority == "P3");
            int completedCount = allItems.Count(i => i.FrontMatter.Status == "done");

            // Stale todo count (> 60 days)
            var now = _timeProvider.GetLocalNow();
            int staleCount = 0;
            foreach (var ai in activeItems)
            {
                if (DateOnly.TryParse(ai.FrontMatter.Created, out var createdDate))
                {
                    if (DateOnly.FromDateTime(now.DateTime).DayNumber - createdDate.DayNumber > 60)
                    {
                        staleCount++;
                    }
                }
            }

            // Cycle time median calculation for done tasks with started & closed dates
            var cycleTimes = new List<int>();
            foreach (var item in allItems.Where(i => i.FrontMatter.Status == "done"))
            {
                if (DateOnly.TryParse(item.FrontMatter.Started, out var sDate) &&
                    DateOnly.TryParse(item.FrontMatter.Closed, out var cDate))
                {
                    var diff = cDate.DayNumber - sDate.DayNumber;
                    if (diff >= 0) cycleTimes.Add(diff);
                }
            }

            string medianCycleTimeStr = "N/A";
            if (cycleTimes.Count > 0)
            {
                cycleTimes.Sort();
                var mid = cycleTimes.Count / 2;
                var median = cycleTimes.Count % 2 != 0
                    ? cycleTimes[mid]
                    : (cycleTimes[mid - 1] + cycleTimes[mid]) / 2.0;
                medianCycleTimeStr = $"{median:F1} days";
            }

            // 2. Sprint Velocity and History (last 6 sprints)
            var sprintHistory = new List<SprintFrontMatter>();
            if (Directory.Exists(sprintsDir))
            {
                var sFiles = Directory.GetFiles(sprintsDir, "*.md");
                foreach (var sf in sFiles)
                {
                    var sText = await File.ReadAllTextAsync(sf, ct).ConfigureAwait(false);
                    if (BacklogRepository.TryParseFrontMatter<SprintFrontMatter>(sText, out var sfm, out _) && sfm != null)
                    {
                        sprintHistory.Add(sfm);
                    }
                }
            }

            var closedSprints = sprintHistory
                .Where(s => s.Status == "closed")
                .OrderByDescending(s => s.End)
                .Take(6)
                .OrderBy(s => s.End)
                .ToList();

            // 3. Module-level Compliance Scanning (Industrial DNA)
            var modules = Directory.Exists(srcDir) ? Directory.GetDirectories(srcDir) : Array.Empty<string>();
            int totalModules = modules.Length;
            int markedModules = 0;
            int diagnosticsModules = 0;
            int testedModules = 0;
            int optionsModules = 0;

            foreach (var module in modules)
            {
                var moduleName = Path.GetFileName(module);
                if (Directory.GetFiles(module, $"*{moduleName}Block.cs", SearchOption.AllDirectories).Length > 0)
                    markedModules++;
                if (Directory.Exists(Path.Combine(module, "Diagnostics")))
                    diagnosticsModules++;

                var testPath = Path.Combine(projectRoot, "test", "BuildingBlocks", $"{moduleName}.UnitTests");
                if (!Directory.Exists(testPath))
                    testPath = Path.Combine(projectRoot, "test", "BuildingBlocks", moduleName);
                if (Directory.Exists(testPath))
                    testedModules++;

                var optionsFiles = Directory.GetFiles(module, "*Options.cs", SearchOption.AllDirectories)
                    .Where(f => !f.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase) && !f.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (optionsFiles.Length > 0)
                    optionsModules++;
            }

            // 4. Codebase Cleanliness & Rule Invariants Scanning
            int ambientViolations = 0;
            int nakedTodos = 0;
            int antiPatternFolderCount = 0;

            if (Directory.Exists(srcDir))
            {
                // Check anti-pattern folders (Utils, Helpers outside Internal) [ARCH-06]
                var subDirs = Directory.GetDirectories(srcDir, "*", SearchOption.AllDirectories);
                foreach (var d in subDirs)
                {
                    var dirName = Path.GetFileName(d);
                    if (dirName.Equals("Utils", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("Managers", StringComparison.OrdinalIgnoreCase) ||
                        (dirName.Equals("Helpers", StringComparison.OrdinalIgnoreCase) && !d.Contains("\\Internal\\", StringComparison.OrdinalIgnoreCase)))
                    {
                        antiPatternFolderCount++;
                    }
                }

                // Sample C# files for ambient calls and naked TODOs [CS.06, DL.02]
                var csFiles = Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories)
                    .Where(f => !f.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase) && !f.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var ambientRegex = new Regex(@"\b(DateTime\.(UtcNow|Now)|Guid\.NewGuid\(\))", RegexOptions.Compiled);
                var nakedTodoRegex = new Regex(@"//\s*TODO\b(?!\[)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                foreach (var file in csFiles)
                {
                    var content = await File.ReadAllTextAsync(file, ct).ConfigureAwait(false);
                    if (ambientRegex.IsMatch(content)) ambientViolations++;
                    if (nakedTodoRegex.IsMatch(content)) nakedTodos++;
                }
            }

            // 5. ADR Asset Count [DL.03, PS.02]
            int adrCount = 0;
            if (Directory.Exists(adrDir))
            {
                adrCount = Directory.GetFiles(adrDir, "adr-*.md", SearchOption.AllDirectories).Length;
            }

            // 6. Comprehensive Score Calculation (0 ~ 100)
            // - Industrial DNA (70%): Marker 20%, Diagnostics 20%, Options 15%, Tests 15%
            // - Cleanliness & Invariants (30%): Ambient Violations 10%, TODO Discipline 10%, Folders 10%
            double dnaScore = totalModules == 0 ? 0 : (
                (markedModules / (double)totalModules * 0.20) +
                (diagnosticsModules / (double)totalModules * 0.20) +
                (optionsModules / (double)totalModules * 0.15) +
                (testedModules / (double)totalModules * 0.15)
            ) * 100;

            double cleanScore = 0.0;
            if (ambientViolations == 0) cleanScore += 10.0;
            else cleanScore += Math.Max(0, 10.0 - (ambientViolations * 2.0));

            if (nakedTodos == 0) cleanScore += 10.0;
            else cleanScore += Math.Max(0, 10.0 - (nakedTodos * 2.0));

            if (antiPatternFolderCount == 0) cleanScore += 10.0;
            else cleanScore += Math.Max(0, 10.0 - (antiPatternFolderCount * 5.0));

            double totalScore = Math.Clamp(dnaScore + cleanScore, 0.0, 100.0);

            // 7. Generate Markdown
            var sb = new StringBuilder();
            sb.AppendLine("<!-- GENERATED: do not edit manually. Generated by vk-blocks-be-manager -->");
            sb.AppendLine("# 📊 Project Health Scorecard");
            sb.AppendLine($"**Generated at**: {now:yyyy-MM-dd HH:mm:ss} (Local)\n");

            sb.AppendLine("## 🛡️ VK.Blocks Architectural Compliance");
            sb.AppendLine($"**Overall Score: {totalScore:F1} / 100**\n");

            sb.AppendLine("### 1. Industrial DNA Maturity (70%)");
            sb.AppendLine("| Metric | Progress | Ratio | Status | Rule |");
            sb.AppendLine("| :--- | :---: | :---: | :---: | :---: |");
            var markerPct = totalModules > 0 ? markedModules / (double)totalModules * 100 : 0;
            var diagPct = totalModules > 0 ? diagnosticsModules / (double)totalModules * 100 : 0;
            var optPct = totalModules > 0 ? optionsModules / (double)totalModules * 100 : 0;
            var testPct = totalModules > 0 ? testedModules / (double)totalModules * 100 : 0;

            sb.AppendLine($"| `[VKBlockMarker]` Source Generator Marker | {markedModules}/{totalModules} | {markerPct:F0}% | {(markedModules == totalModules ? "✅" : "⚠️")} | `BB.02` |");
            sb.AppendLine($"| `Diagnostics/` Structured Logging & Metrics | {diagnosticsModules}/{totalModules} | {diagPct:F0}% | {(diagnosticsModules == totalModules ? "✅" : "⚠️")} | `OR.01, BB.04` |");
            sb.AppendLine($"| Typed Options Standard (`*Options.cs`) | {optionsModules}/{totalModules} | {optPct:F0}% | {(optionsModules >= totalModules * 0.8 ? "✅" : "⚠️")} | `AP.04, BB.05` |");
            sb.AppendLine($"| Standalone Unit Tests Project Coverage | {testedModules}/{totalModules} | {testPct:F0}% | {(testedModules == totalModules ? "✅" : "⚠️")} | `DL.01` |");
            sb.AppendLine();

            sb.AppendLine("### 2. Cleanliness & Zero-Debt Invariants (30%)");
            sb.AppendLine("| Metric | Violations | Target | Status | Rule |");
            sb.AppendLine("| :--- | :---: | :---: | :---: | :---: |");
            sb.AppendLine($"| Ambient Clock / Guid (`DateTime.UtcNow` / `Guid.NewGuid`) | {ambientViolations} | 0 | {(ambientViolations == 0 ? "✅ Pure" : "🚨 Injection Required")} | `CS.06, BASE-02` |");
            sb.AppendLine($"| Naked TODOs (`// TODO` without condition/PBI) | {nakedTodos} | 0 | {(nakedTodos == 0 ? "✅ Clean" : "⚠️ PBI Required")} | `DL.02, DL.04` |");
            sb.AppendLine($"| Anti-Pattern Folders (`Utils/`, `Helpers/` outside Internal) | {antiPatternFolderCount} | 0 | {(antiPatternFolderCount == 0 ? "✅ Clean" : "⚠️ Slice Required")} | `ARCH-06, BB.01` |");
            sb.AppendLine($"| Architectural Decision Records (ADRs) | {adrCount} entries | - | 📚 | `DL.03, PS.02` |");
            sb.AppendLine();

            sb.AppendLine("## 📈 Sprint Velocity & Performance");
            if (closedSprints.Count == 0)
            {
                sb.AppendLine("_過去に完了した Sprint の履歴はありません（初回 Sprint 開始準備完了）_");
            }
            else
            {
                sb.AppendLine("| Sprint | Goal Met | Committed | Completed | Rate | Carried Over |");
                sb.AppendLine("| :--- | :---: | :---: | :---: | :---: | :---: |");
                foreach (var cs in closedSprints)
                {
                    var gm = cs.GoalMet.HasValue ? (cs.GoalMet.Value ? "✅ Yes" : "❌ No") : "-";
                    var comPts = cs.CommittedPoints?.ToString() ?? "-";
                    var cmpPts = cs.CompletedPoints?.ToString() ?? "0";
                    var rate = (cs.CommittedPoints.HasValue && cs.CommittedPoints.Value > 0 && cs.CompletedPoints.HasValue)
                        ? $"{(cs.CompletedPoints.Value / (double)cs.CommittedPoints.Value * 100):F0}%"
                        : "-";
                    sb.AppendLine($"| {cs.Id} | {gm} | {comPts} pts | {cmpPts} pts | {rate} | {cs.CarriedOver.Count} |");
                }
            }
            sb.AppendLine();

            sb.AppendLine("## 📋 Backlog Health & Flow Metrics");
            sb.AppendLine("| Metric | Value | Indicator | Description / 説明 |");
            sb.AppendLine("| :--- | :---: | :---: | :--- |");
            sb.AppendLine($"| P0 (Critical/Urgent) | {p0Count} | {(p0Count == 0 ? "🌱 0" : "🚨 Urgent")} | 最優先ブロッカー負債 (Critical Blocker) |");
            sb.AppendLine($"| P1 (Type A Violation / High) | {p1Count} | 🔴 | Type A 規律違反、優先対応必須 (Architecture Violation) |");
            sb.AppendLine($"| P2 (Type B / Medium Feature) | {p2Count} | 🟡 | 通常の機能追加・リファクタリング (Standard Backlog) |");
            sb.AppendLine($"| P3 (Optimization / Low) | {p3Count} | 🔵 | 次要な最適化・整理 (Minor Improvement) |");
            sb.AppendLine($"| Total Completed (All Time) | {completedCount} | ✅ | 累計完了タスク数 (All-Time Finished PBIs) |");
            sb.AppendLine($"| Cycle Time (Median) | {medianCycleTimeStr} | ⏱️ | 着手から完了までの中央日数 (`doing` -> `done`) |");
            sb.AppendLine($"| Stale Backlogs (> 60 days) | {staleCount} | {(staleCount > 10 ? "⚠️" : "🌱")} | 60日以上更新のないバックログ (Unattended Backlogs) |");
            sb.AppendLine();

            var scorecardPath = Path.Combine(backlogsDir, "Health_Scorecard.md");
            await File.WriteAllTextAsync(scorecardPath, sb.ToString(), ct).ConfigureAwait(false);

            return $"✅ Dashboard generated at {scorecardPath}. Compliance Score: {totalScore:F1} / 100";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"[{McpBacklogErrors.IoFailure}] Failed to generate dashboard: {ex.Message}";
        }
    }
}
