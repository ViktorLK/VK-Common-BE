using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    [McpServerTool]
    [Description("Adds a new atomic task item to the project backlogs in docs/Backlogs/{Module}/.")]
    public static async Task<string> VKBeAddBacklogItem(
        [Description("The module name (e.g., 'Core', 'AI', 'Auth').")] string module,
        [Description("A short, descriptive title for the task.")] string title,
        [Description("Detailed description of what needs to be done.")] string description,
        [Description("Priority level: 'High', 'Medium', or 'Low'.")] string priority = "Medium",
        [Description("Target code or component affected (optional).")] string? target = null,
        [Description("Reference to rules or documents (optional).")] string? reference = null,
        CancellationToken ct = default)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var backlogsDir = Path.Combine(projectRoot, "docs", "05-Backlogs");

            if (!Directory.Exists(backlogsDir))
            {
                Directory.CreateDirectory(backlogsDir);
            }

            // 1. Prepare Module Directory
            var moduleName = module;
            var moduleDir = Path.Combine(backlogsDir, moduleName);
            if (!Directory.Exists(moduleDir))
            {
                Directory.CreateDirectory(moduleDir);
            }

            // 2. Generate Task ID and Filename
            var modulePrefix = module.ToUpperInvariant();
            var existingFiles = Directory.GetFiles(moduleDir, "*.md");
            int nextIndex = 1;
            if (existingFiles.Length > 0)
            {
                var indices = existingFiles
                    .Select(f => Path.GetFileNameWithoutExtension(f).Split('-')[0])
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .ToList();
                if (indices.Count != 0)
                {
                    nextIndex = indices.Max() + 1;
                }
            }

            var taskId = $"{modulePrefix}-{nextIndex:D3}";
            var slug = title.ToLowerInvariant().Replace(" ", "-").Replace(".", "").Replace("/", "-");
            var fileName = $"{nextIndex:D3}-{slug}.md";
            var filePath = Path.Combine(moduleDir, fileName);

            // 3. Create Task File
            var priorityIcon = priority.ToLowerInvariant() switch
            {
                "high" => "🔴",
                "low" => "🔵",
                _ => "🟡"
            };

            var taskContent = $"""
# Task: {title}
**ID**: {taskId}
**Status**: {priorityIcon} {priority} | #Debt
**Target**: `{target ?? "N/A"}`
**Ref**: {reference ?? "N/A"}

## 📝 Description
{description}

## ✅ DoD (Definition of Done)
- [ ] {title}
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests
""";
            await File.WriteAllTextAsync(filePath, taskContent, ct).ConfigureAwait(false);

            // 4. Update Active_Sprint.md
            await UpdateActiveSprintBoard(backlogsDir, moduleName, taskId, title, moduleName, fileName, ct).ConfigureAwait(false);

            return $"[MCP Tool: VKAddBacklogItem]\n✅ Created atomic task: {taskId}\n- **File**: {Path.Combine("docs", "05-Backlogs", moduleName, fileName)}\n- **Status**: Added to Active_Sprint.md";
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to add backlog item: {ex.Message}";
        }
    }

    private static async Task UpdateActiveSprintBoard(string backlogsDir, string moduleName, string taskId, string title, string moduleFolderName, string fileName, CancellationToken ct)
    {
        var sprintFile = Path.Combine(backlogsDir, "Active_Sprint.md");
        if (!File.Exists(sprintFile))
        {
            var initialContent = "# 🚀 Current Sprint\n\n";
            await File.WriteAllTextAsync(sprintFile, initialContent, ct).ConfigureAwait(false);
        }

        var content = (await File.ReadAllLinesAsync(sprintFile, ct).ConfigureAwait(false)).ToList();
        var moduleIcon = moduleName.ToLowerInvariant() switch
        {
            "core" => "🏗️",
            "ai" => "🧠",
            "auth" or "authorization" or "authentication" => "🔐",
            "persistence" or "database" or "db" => "💾",
            "multitenancy" => "💾",
            "infrastructure" => "⚙️",
            _ => "📋"
        };

        var moduleHeader = $"## {moduleIcon} {moduleName} Tasks";
        int headerIndex = content.FindIndex(l => l.StartsWith(moduleHeader, StringComparison.OrdinalIgnoreCase));

        var taskLine = $"- [ ] [{taskId}: {title}](./{moduleFolderName}/{fileName})";

        if (headerIndex == -1)
        {
            // Add new module section
            content.Add("");
            content.Add(moduleHeader);
            content.Add(taskLine);
        }
        else
        {
            // Insert under existing section
            int insertIndex = headerIndex + 1;
            while (insertIndex < content.Count && !content[insertIndex].StartsWith("##"))
            {
                insertIndex++;
            }
            content.Insert(insertIndex, taskLine);
        }

        await File.WriteAllLinesAsync(sprintFile, content, ct).ConfigureAwait(false);
    }

    [McpServerTool]
    [Description("Scans the codebase and backlogs to generate a project health scorecard and compliance dashboard.")]
    public static async Task<string> VKBeGenerateDashboard(CancellationToken ct)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var backlogsDir = Path.Combine(projectRoot, "docs", "05-Backlogs");
            var srcDir = Path.Combine(projectRoot, "src", "BuildingBlocks");

            // 1. Backlog Analysis
            var allBacklogs = Directory.GetFiles(backlogsDir, "*.md", SearchOption.AllDirectories)
                .Where(f => !f.Contains(".Archive") && !Path.GetFileName(f).StartsWith("Active_Sprint") && !Path.GetFileName(f).StartsWith("Health_Scorecard"))
                .ToList();

            int highCount = 0, medCount = 0, lowCount = 0;
            foreach (var file in allBacklogs)
            {
                var text = await File.ReadAllTextAsync(file, ct).ConfigureAwait(false);
                if (text.Contains("🔴 High"))
                    highCount++;
                else if (text.Contains("🟡 Medium"))
                    medCount++;
                else if (text.Contains("🔵 Low"))
                    lowCount++;
            }

            // 2. Compliance Scanning (Basic)
            var modules = Directory.GetDirectories(srcDir);
            int totalModules = modules.Length;
            int markedModules = 0;
            int diagnosticsModules = 0;
            int testedModules = 0;

            foreach (var module in modules)
            {
                var moduleName = Path.GetFileName(module);
                // Check for [moduleName]Block.cs or VK[moduleName]Block.cs
                if (Directory.GetFiles(module, $"*{moduleName}Block.cs", SearchOption.AllDirectories).Any())
                    markedModules++;
                if (Directory.Exists(Path.Combine(module, "Diagnostics")))
                    diagnosticsModules++;

                var testPath = Path.Combine(projectRoot, "test", "BuildingBlocks", $"{moduleName}.UnitTests");
                if (!Directory.Exists(testPath))
                    testPath = Path.Combine(projectRoot, "test", "BuildingBlocks", moduleName); // Fallback
                if (Directory.Exists(testPath))
                    testedModules++;
            }

            // 3. Score Calculation
            double complianceScore = totalModules == 0 ? 0 : (
                (markedModules / (double)totalModules * 0.4) +
                (diagnosticsModules / (double)totalModules * 0.3) +
                (testedModules / (double)totalModules * 0.3)
            ) * 100;

            // 4. Generate Markdown
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("# 📊 Project Health Scorecard");
            sb.AppendLine($"**Generated at**: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

            sb.AppendLine("## 🛡️ Architectural Compliance");
            sb.AppendLine($"**Overall Score: {complianceScore:F1} / 100**\n");
            sb.AppendLine("| Metric | Progress | Status |");
            sb.AppendLine("| :--- | :--- | :--- |");
            sb.AppendLine($"| [VKBlockMarker] Coverage | {markedModules}/{totalModules} | {(markedModules == totalModules ? "✅" : "⚠️")} |");
            sb.AppendLine($"| Diagnostics (Logging/Metrics) | {diagnosticsModules}/{totalModules} | {(diagnosticsModules == totalModules ? "✅" : "⚠️")} |");
            sb.AppendLine($"| Unit Testing Coverage | {testedModules}/{totalModules} | {(testedModules == totalModules ? "✅" : "⚠️")} |");
            sb.AppendLine();

            sb.AppendLine("## 📋 Backlog Velocity");
            sb.AppendLine("| Priority | Count | Emoji |");
            sb.AppendLine("| :--- | :--- | :--- |");
            sb.AppendLine($"| High Priority | {highCount} | 🔴 |");
            sb.AppendLine($"| Medium Priority | {medCount} | 🟡 |");
            sb.AppendLine($"| Low Priority | {lowCount} | 🔵 |");
            sb.AppendLine();

            var scorecardPath = Path.Combine(backlogsDir, "Health_Scorecard.md");
            await File.WriteAllTextAsync(scorecardPath, sb.ToString(), ct).ConfigureAwait(false);

            return $"✅ Dashboard generated at {scorecardPath}. Compliance Score: {complianceScore:F1}";
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to generate dashboard: {ex.Message}";
        }
    }
}


