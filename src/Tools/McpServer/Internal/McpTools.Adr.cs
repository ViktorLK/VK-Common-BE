using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    [McpServerTool]
    [Description("Publishes a new ADR in docs/04-ArchitectureDecisionRecords/{Module}/ following docs/00-Blueprints/04-AdrBlueprint.md. Deterministically calculates the next sequential number, formats a safe filename, updates superseded previous ADRs, writes the ADR markdown, and updates the module README.md index.")]
    public static async Task<string> VKBeDraftAdr(
        [Description("Path or name of the module, e.g. 'src/BuildingBlocks/Authentication' or 'Authentication'.")] string sourceDir,
        [Description("ADR title in English.")] string title,
        [Description("The architectural decision made and its rationale.")] string decision,
        [Description("Background context and problem statement.")] string? context = null,
        [Description("Alternatives considered and rejected reasons.")] string? alternatives = null,
        [Description("Consequences (positive, negative, mitigations).")] string? consequences = null,
        [Description("Implementation details and security considerations.")] string? implementation = null,
        [Description("Optional complete markdown body of the ADR. If provided, used directly.")] string? fullContent = null,
        [Description("Optional previous ADR number/ID this decision supersedes (e.g. '001' or 'ADR-001'). Automatically marks the old ADR as Superseded.")] string? supersedes = null,
        [Description("Japanese 1-2 sentence summary for the README.md index. Defaults to decision excerpt.")] string? summary = null,
        [Description("Comma-separated keywords for the README.md index.")] string? keywords = null,
        [Description("Category under ADR index (defaults to 'Core Architecture & Decisions').")] string? category = null,
        CancellationToken ct = default)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var adrsBaseDir = Path.Combine(projectRoot, "docs", "04-ArchitectureDecisionRecords");

            if (!Directory.Exists(adrsBaseDir))
            {
                Directory.CreateDirectory(adrsBaseDir);
            }

            // 1. Derive and normalize module name
            var normalizedSrc = sourceDir.Replace('\\', '/').TrimEnd('/');
            var moduleName = normalizedSrc.Split('/').LastOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? "General";
            var outputDir = Path.Combine(adrsBaseDir, moduleName);
            Directory.CreateDirectory(outputDir);

            // 2. Sequential numbering (Regex match on adr-NNN-*.md)
            var existingFiles = Directory.GetFiles(outputDir, "adr-*.md");
            var adrRegex = new Regex(@"^adr-(\d+)-.+\.md$", RegexOptions.IgnoreCase);
            var nextIndex = 1;
            if (existingFiles.Length > 0)
            {
                var indices = existingFiles
                    .Select(f => Path.GetFileName(f))
                    .Select(f => adrRegex.Match(f))
                    .Where(m => m.Success)
                    .Select(m => int.Parse(m.Groups[1].Value))
                    .ToList();

                if (indices.Count > 0)
                {
                    nextIndex = indices.Max() + 1;
                }
            }

            var nextNumber = nextIndex.ToString("D3");
            var date = DateTime.Now.ToString("yyyy-MM-dd");

            // 3. Safe Slugification
            var slug = SlugifyTitle(title);
            var fileName = $"adr-{nextNumber}-{slug}.md";
            var outputPath = Path.Combine(outputDir, fileName);

            // 4. Handle Superseding of previous ADR
            string? supersededNotice = null;
            string? supersedesNumber = null;
            if (!string.IsNullOrWhiteSpace(supersedes))
            {
                var numMatch = Regex.Match(supersedes, @"\d+");
                if (numMatch.Success)
                {
                    supersedesNumber = int.Parse(numMatch.Value).ToString("D3");
                    var targetPattern = $"adr-{supersedesNumber}-*.md";
                    var oldFiles = Directory.GetFiles(outputDir, targetPattern);
                    if (oldFiles.Length > 0)
                    {
                        var oldFilePath = oldFiles[0];
                        var oldContent = await File.ReadAllTextAsync(oldFilePath, ct).ConfigureAwait(false);
                        var updatedOld = Regex.Replace(
                            oldContent,
                            @"\*\*Status\*\*:\s*[^\r\n]+",
                            $"**Status**: ❌ Superseded by [ADR-{nextNumber}](./{fileName})"
                        );
                        await File.WriteAllTextAsync(oldFilePath, updatedOld, ct).ConfigureAwait(false);
                        supersededNotice = $"ADR-{supersedesNumber} ({Path.GetFileName(oldFilePath)}) marked as Superseded";
                    }
                }
            }

            // 5. Construct ADR Content
            var statusSuffix = supersedesNumber != null ? $" (Supersedes ADR-{supersedesNumber})" : "";
            var adrContent = fullContent;
            if (string.IsNullOrWhiteSpace(adrContent))
            {
                var sb = new StringBuilder();
                sb.AppendLine($"# ADR {nextNumber}: {title}");
                sb.AppendLine();
                sb.AppendLine($"**Date**: {date}");
                sb.AppendLine($"**Status**: ✅ Accepted{statusSuffix}");
                sb.AppendLine("**Deciders**: Architecture Team");
                sb.AppendLine($"**Technical Story**: {moduleName}");
                sb.AppendLine();
                sb.AppendLine("## 1. Context (背景)");
                sb.AppendLine(context ?? "N/A");
                sb.AppendLine();
                sb.AppendLine("## 2. Decision (決定事項)");
                sb.AppendLine(decision);
                sb.AppendLine();
                sb.AppendLine("## 3. Alternatives Considered (代替案の検討)");
                sb.AppendLine(alternatives ?? "- **Option 1**: Retain legacy status quo (Rejected: insufficient modularity and maintainability).");
                sb.AppendLine();
                sb.AppendLine("## 4. Consequences & Mitigation (結果と緩和策)");
                sb.AppendLine(consequences ?? "- **Positive**: Enforces deterministic and modular architecture.\n- **Negative**: Initial adaptation overhead.\n- **Mitigation**: Standardized through VK.Blocks guidance.");
                sb.AppendLine();
                sb.AppendLine("## 5. Implementation & Security (実装詳細とセキュリティ考察)");
                sb.AppendLine(implementation ?? "Follows standard VK.Blocks architectural patterns and boundaries.");

                adrContent = sb.ToString();
            }
            else
            {
                // If fullContent does not start with standard metadata, prepend title & metadata
                if (!adrContent.TrimStart().StartsWith("# ADR", StringComparison.OrdinalIgnoreCase))
                {
                    var header = $"""
# ADR {nextNumber}: {title}

**Date**: {date}
**Status**: ✅ Accepted{statusSuffix}
**Deciders**: Architecture Team
**Technical Story**: {moduleName}


""";
                    adrContent = header + adrContent;
                }
            }

            // Write the new ADR file
            await File.WriteAllTextAsync(outputPath, adrContent, ct).ConfigureAwait(false);

            // 6. Update README.md in output directory
            var effectiveSummary = !string.IsNullOrWhiteSpace(summary)
                ? summary
                : decision.Length > 150 ? decision.Substring(0, 147) + "..." : decision;
            var effectiveKeywords = !string.IsNullOrWhiteSpace(keywords) ? keywords : $"{moduleName}, Architecture";
            var effectiveCategory = !string.IsNullOrWhiteSpace(category) ? category : "Core Architecture & Decisions";

            await UpdateModuleAdrReadme(outputDir, moduleName, nextNumber, title, fileName, statusSuffix, effectiveSummary, effectiveKeywords, effectiveCategory, supersedesNumber, date, ct).ConfigureAwait(false);

            var totalCount = Directory.GetFiles(outputDir, "adr-*.md").Length;
            var relPath = Path.Combine("docs", "04-ArchitectureDecisionRecords", moduleName, fileName);
            var readmeRelPath = Path.Combine("docs", "04-ArchitectureDecisionRecords", moduleName, "README.md");

            var resultSb = new StringBuilder();
            resultSb.AppendLine("[MCP Tool: VKBeDraftAdr]");
            resultSb.AppendLine($"✅ Published ADR: ADR-{nextNumber} ({title})");
            resultSb.AppendLine($"- **File**: {relPath}");
            resultSb.AppendLine($"- **Status**: ✅ Accepted{statusSuffix}");
            resultSb.AppendLine($"- **Index**: Updated {readmeRelPath} (Total ADRs: {totalCount})");
            if (supersededNotice != null)
            {
                resultSb.AppendLine($"- **Superseded**: {supersededNotice}");
            }

            return resultSb.ToString().TrimEnd();
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to publish ADR: {ex.Message}";
        }
    }

    private static string SlugifyTitle(string title)
    {
        var clean = Regex.Replace(title.ToLowerInvariant(), @"[^a-z0-9\s-]", "");
        clean = Regex.Replace(clean, @"\s+", "-").Trim('-');
        if (clean.Length > 60)
        {
            clean = clean.Substring(0, 60).TrimEnd('-');
        }
        return string.IsNullOrWhiteSpace(clean) ? "decision" : clean;
    }

    private static async Task UpdateModuleAdrReadme(
        string outputDir,
        string moduleName,
        string nextNumber,
        string title,
        string fileName,
        string statusSuffix,
        string summary,
        string keywords,
        string category,
        string? supersedesNumber,
        string date,
        CancellationToken ct)
    {
        var readmePath = Path.Combine(outputDir, "README.md");
        var card = $"""
#### [ADR-{nextNumber}: {title}](./{fileName})

**Status**: ✅ Accepted{statusSuffix}  
**概要**: {summary}  
**キーワード**: {keywords}

---

""";

        var allAdrsCount = Directory.GetFiles(outputDir, "adr-*.md").Length;

        if (!File.Exists(readmePath))
        {
            var initialContent = $"""
# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.{moduleName} モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### {category}

{card}
## 🎯 ADR の読み方ガイド

### 設計決定の理解用
1. **ADR-{nextNumber}**: {summary}

---

**Last Updated**: {date}  
**Total ADRs**: {allAdrsCount}
""";
            await File.WriteAllTextAsync(readmePath, initialContent, ct).ConfigureAwait(false);
            return;
        }

        var readme = await File.ReadAllTextAsync(readmePath, ct).ConfigureAwait(false);

        // If an old ADR was superseded, update its status in the README as well
        if (!string.IsNullOrWhiteSpace(supersedesNumber))
        {
            readme = Regex.Replace(
                readme,
                $@"(\[ADR-{supersedesNumber}:[^\]]+\]\([^\)]+\)\s*\r?\n\s*\*\*Status\*\*:\s*)[^\r\n]+",
                $"$1❌ Superseded by [ADR-{nextNumber}](./{fileName})"
            );
        }

        // Insert card into ADR list
        var categoryHeader = $"### {category}";
        if (readme.Contains(categoryHeader, StringComparison.OrdinalIgnoreCase))
        {
            var idx = readme.IndexOf(categoryHeader, StringComparison.OrdinalIgnoreCase);
            var endOfHeader = readme.IndexOf('\n', idx);
            if (endOfHeader > 0)
            {
                readme = readme.Insert(endOfHeader + 1, "\n" + card);
            }
        }
        else if (readme.Contains("## 📚 ADR 一覧", StringComparison.OrdinalIgnoreCase))
        {
            var idx = readme.IndexOf("## 📚 ADR 一覧", StringComparison.OrdinalIgnoreCase);
            var endOfHeader = readme.IndexOf('\n', idx);
            if (endOfHeader > 0)
            {
                readme = readme.Insert(endOfHeader + 1, $"\n### {category}\n\n" + card);
            }
        }
        else
        {
            readme += $"\n\n## 📚 ADR 一覧\n\n### {category}\n\n{card}";
        }

        // Append to reading guide if present
        if (readme.Contains("## 🎯 ADR の読み方ガイド", StringComparison.OrdinalIgnoreCase))
        {
            var guideEntry = $"- **ADR-{nextNumber}**: {summary}\n";
            var lastUpdatedIdx = readme.IndexOf("**Last Updated**", StringComparison.OrdinalIgnoreCase);
            if (lastUpdatedIdx > 0)
            {
                readme = readme.Insert(lastUpdatedIdx, guideEntry + "\n");
            }
        }

        // Update Total ADRs and Last Updated
        allAdrsCount = Directory.GetFiles(outputDir, "adr-*.md").Length;
        if (Regex.IsMatch(readme, @"\*\*Total ADRs\*\*:\s*\d+"))
        {
            readme = Regex.Replace(readme, @"\*\*Total ADRs\*\*:\s*\d+", $"**Total ADRs**: {allAdrsCount}");
        }
        else
        {
            readme += $"\n**Total ADRs**: {allAdrsCount}\n";
        }

        if (Regex.IsMatch(readme, @"\*\*Last Updated\*\*:\s*[^\r\n]+"))
        {
            readme = Regex.Replace(readme, @"\*\*Last Updated\*\*:\s*[^\r\n]+", $"**Last Updated**: {date}");
        }
        else
        {
            readme += $"\n**Last Updated**: {date}\n";
        }

        await File.WriteAllTextAsync(readmePath, readme, ct).ConfigureAwait(false);
    }
}
