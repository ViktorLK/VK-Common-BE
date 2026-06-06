using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    [McpServerTool]
    [Description("Generates an ADR draft based on the official rulebook. Returns a prompt instructing the AI to write and save the final .md files.")]
    public static async Task<string> VKBeDraftArchitectureDecisionRecord(
        [Description("Path to the source code directory being documented (e.g. 'src/BuildingBlocks/Authentication'). Determines the output subdirectory.")] string sourceDir,
        [Description("ADR title in English.")] string title,
        [Description("Background context: why is this decision needed?")] string context,
        [Description("The architectural decision made and its rationale.")] string decision,
        CancellationToken ct)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var templatePath = Path.Combine(projectRoot, "docs", "00-Blueprints", "AdrPrompt.md");

            if (!File.Exists(templatePath))
            {
                return $"[Error] ADR template not found at: {templatePath}";
            }

            var adrRules = await File.ReadAllTextAsync(templatePath, ct).ConfigureAwait(false);

            // Derive module name
            var normalizedSrc = sourceDir.Replace('\\', '/').TrimEnd('/');
            var moduleName = normalizedSrc.Split('/').LastOrDefault() ?? "General";

            var outputDir = Path.Combine(projectRoot, "docs", "02-ArchitectureDecisionRecords", moduleName);
            Directory.CreateDirectory(outputDir);

            // Sequential numbering
            var existingFiles = Directory.GetFiles(outputDir, "adr-*.md");
            var adrRegex = new Regex(@"^adr-(\d+)-.+\.md$");
            var nextNumber = (existingFiles.Count(f => adrRegex.IsMatch(Path.GetFileName(f))) + 1).ToString("D3");

            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var slug = title.ToLowerInvariant().Replace(" ", "-").Replace("/", "-"); // Simple slugify
            var fileName = $"adr-{nextNumber}-{slug}.md";
            var outputPath = Path.Combine(outputDir, fileName);
            var readmePath = Path.Combine(outputDir, "README.md");

            var existingReadme = "";
            if (File.Exists(readmePath))
            {
                existingReadme = await File.ReadAllTextAsync(readmePath, ct).ConfigureAwait(false);
            }
            else
            {
                existingReadme = $"# ADR Index — {moduleName}\n\n| # | Title | Date | Status |\n|---|-------|------|--------|\n";
            }

            var prompt = $@"
[MCP Tool: publish_adr]
You are acting as a senior .NET software architect.

Your task is TWO steps — complete BOTH in order:

=== STEP 1: Write the ADR file ===
Generate a complete, professional ADR following ALL rules below, then save it.

--- ADR RULES (from docs/00-Blueprints/AdrPrompt.md) ---
{adrRules}

--- USER INPUT ---
- **ADR Number**: {nextNumber}
- **Source Module**: `{normalizedSrc}`
- **Title**: {title}
- **Context / Background**: {context}
- **Decision**: {decision}
- **Date**: {date}

SAVE the ADR to:
  {outputPath}

=== STEP 2: Update the README index ===
The directory README.md tracks all ADRs in this module.
Current README content:
```
{existingReadme}
```

Append a new entry for this ADR under the most appropriate category. Use this EXACT format:

#### [ADR-{nextNumber}: {title}](./{fileName})

**Status**: 📝 Draft
**概要**: [Brief 1-2 sentence summary in Japanese]
**キーワード**: [2-3 relevant tags]

---

Then SAVE the updated README to:
  {readmePath}

Do NOT skip either step. Do NOT just print the content in chat.
";
            return prompt.Trim();
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to prepare ADR draft: {ex.Message}";
        }
    }
}

