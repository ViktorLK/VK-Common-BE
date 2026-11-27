using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Tools.McpServer.Internal.Manifest;

namespace VK.Tools.McpServer.Internal.Agents;

/// <summary>
/// Deterministic governance linter for .agents rules, skills, index links, and external pitfall files.
/// </summary>
internal static partial class AgentsGovernanceLinter
{
    [GeneratedRegex(@"\b(vk_be_[a-z0-9_]+|vk_get_[a-z0-9_]+)\b", RegexOptions.CultureInvariant)]
    private static partial Regex McpToolNameRegex();

    [GeneratedRegex(@"\]\((/(?:\.agents|docs|src)/[^)\s#]+)(?:#[^)\s]*)?\)", RegexOptions.CultureInvariant)]
    private static partial Regex RootRelativeLinkRegex();

    [GeneratedRegex(@"\]\(\.\./", RegexOptions.CultureInvariant)]
    private static partial Regex UpwardRelativeLinkRegex();

    [GeneratedRegex(@"(?<![a-zA-Z0-9_.-])/(vk-[a-z0-9-]+)(\*)?", RegexOptions.CultureInvariant)]
    private static partial Regex SlashSkillCommandRegex();

    [GeneratedRegex(@"Active:\s*\[L1\+L2:", RegexOptions.CultureInvariant)]
    private static partial Regex LegacyHandshakeRegex();

    [GeneratedRegex(@"<rule\s+[^>]*?id\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RuleTagIdRegex();

    [GeneratedRegex(@"`[^`]*`", RegexOptions.CultureInvariant)]
    private static partial Regex InlineCodeSpanRegex();

    [GeneratedRegex(@"\b((?:CS|OR|DL|AP|BB|PS)\.\d{2})\b", RegexOptions.CultureInvariant)]
    private static partial Regex L3RuleIdRegex();

    [GeneratedRegex(@"^#{2,3}\s+[A-Z0-9.]+(:.*)?$", RegexOptions.CultureInvariant)]
    private static partial Regex AllCapsPitfallHeadingRegex();

    public static async Task<AgentsLintReport> RunAsync(
        string projectRoot,
        IReadOnlySet<string> registeredToolNames,
        CancellationToken ct)
    {
        VKGuard.NotNullOrWhiteSpace(projectRoot);
        VKGuard.NotNull(registeredToolNames);

        var findings = new List<AgentsLintFinding>();
        var agentsDir = Path.Combine(projectRoot, ".agents");

        if (!Directory.Exists(agentsDir))
        {
            findings.Add(Error("AGT-002", ".agents", 0, "Directory '.agents' not found at repository root."));
            return new AgentsLintReport
            {
                FilesScanned = 0,
                SkillsCount = 0,
                RegisteredToolsCount = registeredToolNames.Count,
                Findings = findings
            };
        }

        var allAgentMarkdownFiles = Directory.GetFiles(agentsDir, "*.md", SearchOption.AllDirectories)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var readmePath = Path.Combine(agentsDir, "README.md");
        var ruleFiles = Directory.Exists(Path.Combine(agentsDir, "rules"))
            ? Directory.GetFiles(Path.Combine(agentsDir, "rules"), "*.md").OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray()
            : [];

        var skillsDir = Path.Combine(agentsDir, "skills");
        var skillDirs = Directory.Exists(skillsDir)
            ? Directory.GetDirectories(skillsDir).OrderBy(d => d, StringComparer.OrdinalIgnoreCase).ToArray()
            : [];

        var coreGovernanceFiles = new List<string>();
        if (File.Exists(readmePath))
        {
            coreGovernanceFiles.Add(readmePath);
        }

        coreGovernanceFiles.AddRange(ruleFiles);
        coreGovernanceFiles.AddRange(
            skillDirs.Select(d => Path.Combine(d, "SKILL.md")).Where(File.Exists));

        // AGT-001 & AGT-002: Tool names and root-relative links in core governance docs
        foreach (var file in coreGovernanceFiles)
        {
            ct.ThrowIfCancellationRequested();
            var lines = await File.ReadAllLinesAsync(file, ct).ConfigureAwait(false);
            var rel = Relative(projectRoot, file);

            LintToolNames(lines, rel, registeredToolNames, findings);
            LintRootRelativeLinks(lines, rel, projectRoot, findings);
        }

        // AGT-003 & AGT-006: Upward relative links and legacy Handshake across all .agents/*.md
        foreach (var file in allAgentMarkdownFiles)
        {
            ct.ThrowIfCancellationRequested();
            var lines = await File.ReadAllLinesAsync(file, ct).ConfigureAwait(false);
            var rel = Relative(projectRoot, file);

            LintUpwardLinks(lines, rel, findings);
            LintLegacyHandshake(lines, rel, findings);
        }

        // AGT-004: Skill directory contract (SKILL.md + frontmatter name/description)
        var validSkills = await LintSkillsAsync(projectRoot, skillDirs, findings, ct).ConfigureAwait(false);

        // AGT-005: README skill index synchronization
        if (File.Exists(readmePath))
        {
            var readmeLines = await File.ReadAllLinesAsync(readmePath, ct).ConfigureAwait(false);
            LintReadmeSkillIndex(readmeLines, Relative(projectRoot, readmePath), validSkills, findings);
        }

        // AGT-007: L3 Rule ID definitions vs l3-vk-be.md index & dynamic loading table
        await LintL3RuleIdsAsync(projectRoot, agentsDir, ruleFiles, findings, ct).ConfigureAwait(false);

        // AGT-008: External pitfall files heading convention
        var pitfallFiles = DiscoverPitfallFiles(projectRoot);
        foreach (var pitfallFile in pitfallFiles)
        {
            ct.ThrowIfCancellationRequested();
            var lines = await File.ReadAllLinesAsync(pitfallFile, ct).ConfigureAwait(false);
            LintPitfallHeadings(lines, Relative(projectRoot, pitfallFile), findings);
        }

        return new AgentsLintReport
        {
            FilesScanned = allAgentMarkdownFiles.Length + pitfallFiles.Count,
            SkillsCount = skillDirs.Length,
            RegisteredToolsCount = registeredToolNames.Count,
            Findings = findings
        };
    }

    private static void LintToolNames(
        string[] lines,
        string relFile,
        IReadOnlySet<string> registeredToolNames,
        List<AgentsLintFinding> findings)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            foreach (Match m in McpToolNameRegex().Matches(lines[i]))
            {
                var toolName = m.Groups[1].Value;
                if (!registeredToolNames.Contains(toolName))
                {
                    findings.Add(Error(
                        "AGT-001",
                        relFile,
                        i + 1,
                        $"Unknown or legacy MCP tool name '{toolName}'. Verify against registered vk-blocks-be-manager tools."));
                }
            }
        }
    }

    private static void LintRootRelativeLinks(
        string[] lines,
        string relFile,
        string projectRoot,
        List<AgentsLintFinding> findings)
    {
        var inCodeBlock = false;
        for (int i = 0; i < lines.Length; i++)
        {
            if (IsFenceLine(lines[i]))
            {
                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (inCodeBlock)
            {
                continue;
            }

            var lineWithoutInlineCode = InlineCodeSpanRegex().Replace(lines[i], string.Empty);
            foreach (Match m in RootRelativeLinkRegex().Matches(lineWithoutInlineCode))
            {
                var target = m.Groups[1].Value;
                if (IsTemplatePath(target))
                {
                    continue;
                }

                var trimmed = target.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var abs = Path.Combine(projectRoot, trimmed);
                if (!File.Exists(abs) && !Directory.Exists(abs))
                {
                    findings.Add(Error(
                        "AGT-002",
                        relFile,
                        i + 1,
                        $"Broken repository-root-relative link target '{target}'."));
                }
            }
        }
    }

    private static void LintUpwardLinks(string[] lines, string relFile, List<AgentsLintFinding> findings)
    {
        var inCodeBlock = false;
        for (int i = 0; i < lines.Length; i++)
        {
            if (IsFenceLine(lines[i]))
            {
                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (!inCodeBlock && UpwardRelativeLinkRegex().IsMatch(InlineCodeSpanRegex().Replace(lines[i], string.Empty)))
            {
                findings.Add(Warn(
                    "AGT-003",
                    relFile,
                    i + 1,
                    "Upward relative link ('../') violates BASE-11; use repository-root-relative paths (/docs/..., /src/..., /.agents/...)."));
            }
        }
    }

    private static void LintLegacyHandshake(string[] lines, string relFile, List<AgentsLintFinding> findings)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (LegacyHandshakeRegex().IsMatch(lines[i]))
            {
                findings.Add(Error(
                    "AGT-006",
                    relFile,
                    i + 1,
                    "Legacy handshake format 'Active: [L1+L2:...' found; use 'Active: [L0-L3:{Module}] | Context: {Path} | Sync: [{RuleID},...]'."));
            }
        }
    }

    private static async Task<HashSet<string>> LintSkillsAsync(
        string projectRoot,
        string[] skillDirs,
        List<AgentsLintFinding> findings,
        CancellationToken ct)
    {
        var existingSkillNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var dir in skillDirs)
        {
            ct.ThrowIfCancellationRequested();
            var dirName = Path.GetFileName(dir);
            var skillFile = Path.Combine(dir, "SKILL.md");
            var relSkillFile = Relative(projectRoot, skillFile);

            if (!File.Exists(skillFile))
            {
                findings.Add(Error("AGT-004", Relative(projectRoot, dir), 0, $"Skill directory '{dirName}' is missing SKILL.md."));
                continue;
            }

            existingSkillNames.Add(dirName);
            var lines = await File.ReadAllLinesAsync(skillFile, ct).ConfigureAwait(false);
            var (name, description) = ParseFrontmatter(lines);

            if (!string.Equals(name, dirName, StringComparison.Ordinal))
            {
                findings.Add(Error(
                    "AGT-004",
                    relSkillFile,
                    1,
                    $"Skill frontmatter 'name: {name ?? "<missing>"}' must match directory name '{dirName}'."));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                findings.Add(Error("AGT-004", relSkillFile, 1, "Skill frontmatter 'description' is missing or empty."));
            }
        }

        return existingSkillNames;
    }

    private static void LintReadmeSkillIndex(
        string[] readmeLines,
        string relReadme,
        HashSet<string> existingSkillNames,
        List<AgentsLintFinding> findings)
    {
        var referencedSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var inCodeBlock = false;

        for (int i = 0; i < readmeLines.Length; i++)
        {
            if (IsFenceLine(readmeLines[i]))
            {
                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (inCodeBlock)
            {
                continue;
            }

            foreach (Match m in SlashSkillCommandRegex().Matches(readmeLines[i]))
            {
                if (m.Groups[2].Success)
                {
                    // Skip wildcard prefixes like /vk-audit-* or /vk-draft-*
                    continue;
                }

                var skillName = m.Groups[1].Value;
                referencedSkills.Add(skillName);

                if (!existingSkillNames.Contains(skillName))
                {
                    findings.Add(Warn(
                        "AGT-005",
                        relReadme,
                        i + 1,
                        $"Command '/{skillName}' referenced in .agents/README.md does not match any directory under .agents/skills/."));
                }
            }
        }

        foreach (var skill in existingSkillNames.OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
        {
            if (!referencedSkills.Contains(skill))
            {
                findings.Add(Warn(
                    "AGT-005",
                    relReadme,
                    0,
                    $"Skill '{skill}' exists under .agents/skills/ but is not indexed in .agents/README.md."));
            }
        }
    }

    private static async Task LintL3RuleIdsAsync(
        string projectRoot,
        string agentsDir,
        string[] ruleFiles,
        List<AgentsLintFinding> findings,
        CancellationToken ct)
    {
        var definedRuleIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in ruleFiles)
        {
            var fileName = Path.GetFileName(file);
            if (!fileName.StartsWith("l3-vk-be-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var content = await File.ReadAllTextAsync(file, ct).ConfigureAwait(false);
            foreach (Match m in RuleTagIdRegex().Matches(content))
            {
                definedRuleIds.Add(m.Groups[1].Value.Trim());
            }
        }

        var l3IndexPath = Path.Combine(agentsDir, "rules", "l3-vk-be.md");
        if (!File.Exists(l3IndexPath))
        {
            return;
        }

        var relL3 = Relative(projectRoot, l3IndexPath);
        var lines = await File.ReadAllLinesAsync(l3IndexPath, ct).ConfigureAwait(false);
        var inCheckedSection = false;

        for (int i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].Trim();
            if (trimmed.Equals("<rule_index>", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("<dynamic_loading_protocol>", StringComparison.OrdinalIgnoreCase))
            {
                inCheckedSection = true;
                continue;
            }

            if (trimmed.Equals("</rule_index>", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("</dynamic_loading_protocol>", StringComparison.OrdinalIgnoreCase))
            {
                inCheckedSection = false;
                continue;
            }

            if (!inCheckedSection || !trimmed.StartsWith('|'))
            {
                continue;
            }

            foreach (Match m in L3RuleIdRegex().Matches(trimmed))
            {
                var id = m.Groups[1].Value;
                if (!definedRuleIds.Contains(id))
                {
                    findings.Add(Error(
                        "AGT-007",
                        relL3,
                        i + 1,
                        $"L3 rule ID '{id}' is referenced in l3-vk-be.md but has no <rule id=\"{id}\"> definition in l3-vk-be-*.md."));
                }
            }
        }
    }

    private static List<string> DiscoverPitfallFiles(string projectRoot)
    {
        var files = new List<string>();
        var globalPitfalls = Path.Combine(projectRoot, "src", ".prompts", "pitfalls.md");
        if (File.Exists(globalPitfalls))
        {
            files.Add(globalPitfalls);
        }

        var blocksDir = Path.Combine(projectRoot, "src", "BuildingBlocks");
        if (Directory.Exists(blocksDir))
        {
            foreach (var dir in Directory.GetDirectories(blocksDir))
            {
                var modulePitfalls = Path.Combine(dir, ".prompts", "pitfalls.md");
                if (File.Exists(modulePitfalls))
                {
                    files.Add(modulePitfalls);
                }
            }
        }

        return files;
    }

    private static void LintPitfallHeadings(string[] lines, string relFile, List<AgentsLintFinding> findings)
    {
        var inCodeBlock = false;
        for (int i = 0; i < lines.Length; i++)
        {
            if (IsFenceLine(lines[i]))
            {
                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (!inCodeBlock && AllCapsPitfallHeadingRegex().IsMatch(lines[i].Trim()))
            {
                findings.Add(Warn(
                    "AGT-008",
                    relFile,
                    i + 1,
                    "All-caps heading in pitfalls.md is parsed as a Rule ID by McpTools.SplitBodyIntoContext; use Title Case (e.g. '## Tried (...)')."));
            }
        }
    }

    private static (string? Name, string? Description) ParseFrontmatter(string[] lines)
    {
        if (lines.Length < 3 || lines[0].Trim() != "---")
        {
            return (null, null);
        }

        string? name = null;
        string? description = null;

        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].Trim() == "---")
            {
                break;
            }

            var parts = lines[i].Split(':', 2);
            if (parts.Length != 2)
            {
                continue;
            }

            var key = parts[0].Trim();
            var value = parts[1].Trim().Trim('"', '\'');

            if (string.Equals(key, "name", StringComparison.OrdinalIgnoreCase))
            {
                name = value;
            }
            else if (string.Equals(key, "description", StringComparison.OrdinalIgnoreCase))
            {
                description = value;
            }
        }

        return (name, description);
    }

    private static bool IsFenceLine(string line) => line.TrimStart().StartsWith("```", StringComparison.Ordinal);

    private static bool IsTemplatePath(string path) =>
        path.IndexOfAny(['{', '}', '<', '>', '*', '[', ']']) >= 0 ||
        path.Contains("...", StringComparison.Ordinal);

    private static string Relative(string root, string fullPath) =>
        Path.GetRelativePath(root, fullPath).Replace('\\', '/');

    private static AgentsLintFinding Error(string code, string file, int line, string message) =>
        new() { Code = code, Severity = ManifestSeverity.Error, File = file, Line = line, Message = message };

    private static AgentsLintFinding Warn(string code, string file, int line, string message) =>
        new() { Code = code, Severity = ManifestSeverity.Warning, File = file, Line = line, Message = message };
}
