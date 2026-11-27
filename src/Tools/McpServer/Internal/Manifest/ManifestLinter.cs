using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using VK.Blocks.Core;

namespace VK.Tools.McpServer.Internal.Manifest;

/// <summary>
/// Validates module manifests in three layers:
/// A. schema (attributes, sections, limits, invariant IDs),
/// B. dependencies (csproj references against <c>allowed</c>/<c>forbidden</c>),
/// C. ownership (manifest Owns/Delegates against the ownership map).
/// </summary>
internal static partial class ManifestLinter
{
    private const string ManifestRelativePath = ".prompts/module-manifest.md";
    private const int PublicSurfaceMaxLines = 5;
    private const int DefaultsMaxLines = 3;
    private const int InvariantsMaxRules = 15;

    private static readonly string[] RequiredAttributes = ["requires", "allowed", "forbidden"];

    private static readonly string[] RequiredSections =
        ["Ownership", "Boundaries", "Public Surface", "Defaults & Overrides", "Invariants"];

    private const string OptionalSection = "Registration & Diagnostics";

    private static readonly HashSet<string> ReservedPrefixes =
        new(["CS", "AP", "BB", "OR", "DL", "PS", "OWN", "BASE", "ARCH", "FS"], StringComparer.Ordinal);

    public static async Task<ManifestLintReport> RunAsync(
        string projectRoot,
        string modulePattern,
        string mapPath,
        CancellationToken ct)
    {
        VKGuard.NotNullOrWhiteSpace(projectRoot);
        VKGuard.NotNullOrWhiteSpace(modulePattern);
        VKGuard.NotNullOrWhiteSpace(mapPath);

        var findings = new List<ManifestFinding>();
        var blocksRoot = Path.Combine(projectRoot, "src", "BuildingBlocks");
        var allModules = Directory.Exists(blocksRoot)
            ? Directory.GetDirectories(blocksRoot).Select(Path.GetFileName).OfType<string>().ToHashSet(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var patterns = SplitList(modulePattern);
        var modules = allModules
            .Where(m => File.Exists(Path.Combine(blocksRoot, m, ManifestRelativePath)) && patterns.Any(p => GlobMatch(p, m)))
            .OrderBy(m => m, StringComparer.Ordinal)
            .ToList();

        var absMapPath = Path.IsPathRooted(mapPath) ? mapPath : Path.Combine(projectRoot, mapPath);
        var mapRel = Relative(projectRoot, absMapPath);
        OwnershipMap? map = null;
        if (File.Exists(absMapPath))
        {
            map = OwnershipMap.Parse(await File.ReadAllLinesAsync(absMapPath, ct).ConfigureAwait(false));
            LintMap(map, mapRel, allModules, findings);
        }
        else
        {
            findings.Add(Error("MAN-030", mapRel, 0, "Ownership map not found; ownership checks skipped."));
        }

        var claims = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in modules)
        {
            var moduleDir = Path.Combine(blocksRoot, module);
            var file = Relative(projectRoot, Path.Combine(moduleDir, ManifestRelativePath));
            var lines = await File.ReadAllLinesAsync(Path.Combine(moduleDir, ManifestRelativePath), ct).ConfigureAwait(false);

            if (lines.All(string.IsNullOrWhiteSpace))
            {
                findings.Add(Error("MAN-001", file, 0, "Manifest is empty."));
                continue;
            }

            var doc = ManifestDocument.Parse(lines);
            if (doc is null)
            {
                findings.Add(Error("MAN-002", file, 0, "Opening <manifest> tag not found."));
                continue;
            }

            LintSchema(module, doc, file, map, findings);
            await LintDependenciesAsync(projectRoot, moduleDir, doc, file, findings, ct).ConfigureAwait(false);
            if (map is not null)
            {
                claims[module] = LintOwnership(module, doc, file, map, findings);
            }
        }

        if (map is not null)
        {
            LintCoverage(map, mapRel, claims, findings);
        }

        return new ManifestLintReport { Modules = modules, Findings = findings };
    }

    // ---- A. Schema -------------------------------------------------------------------------

    private static void LintSchema(string module, ManifestDocument doc, string file, OwnershipMap? map, List<ManifestFinding> findings)
    {
        if (!doc.IsClosed)
        {
            findings.Add(Error("MAN-002", file, doc.TagLine, "Closing </manifest> tag not found."));
        }

        foreach (var attr in RequiredAttributes.Where(a => !doc.Attributes.ContainsKey(a)))
        {
            findings.Add(Error("MAN-003", file, doc.TagLine, $"Missing attribute '{attr}'."));
        }

        if (doc.Attributes.TryGetValue("requires", out var requires))
        {
            foreach (var rule in SplitList(requires).Where(r => !L3RuleRegex().IsMatch(r)))
            {
                findings.Add(Error("MAN-007", file, doc.TagLine, $"requires '{rule}' is not an L3 rule ID (CS/AP/BB/OR/DL/PS.NN)."));
            }
        }

        LintSections(doc, file, findings);
        LintInvariants(module, doc, file, map, findings);
    }

    private static void LintSections(ManifestDocument doc, string file, List<ManifestFinding> findings)
    {
        var expected = RequiredSections.Append(OptionalSection).ToList();
        var lastIndex = -1;
        foreach (var section in doc.Sections)
        {
            var index = expected.FindIndex(e => e.Equals(section.Title, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                findings.Add(Warning("MAN-014", file, section.Line, $"Unknown section '{section.Title}'."));
                continue;
            }

            if (index < lastIndex)
            {
                findings.Add(Error("MAN-010", file, section.Line, $"Section '{section.Title}' is out of order."));
            }

            lastIndex = Math.Max(lastIndex, index);
        }

        foreach (var required in RequiredSections.Where(r => FindSection(doc, r) is null))
        {
            findings.Add(Error("MAN-010", file, 0, $"Missing section '{required}'."));
        }

        CheckLimit(FindSection(doc, "Public Surface"), PublicSurfaceMaxLines);
        CheckLimit(FindSection(doc, "Defaults & Overrides"), DefaultsMaxLines);

        void CheckLimit(ManifestSection? section, int max)
        {
            var count = section?.Lines.Count(l => !l.Text.StartsWith('#')) ?? 0;
            if (section is not null && count > max)
            {
                findings.Add(Error("MAN-011", file, section.Line, $"Section '{section.Title}' has {count} lines (max {max})."));
            }
        }
    }

    private static void LintInvariants(string module, ManifestDocument doc, string file, OwnershipMap? map, List<ManifestFinding> findings)
    {
        var section = FindSection(doc, "Invariants");
        if (section is null)
        {
            return;
        }

        string? registered = null;
        if (map is not null)
        {
            if (map.Prefixes.TryGetValue(module, out var entry))
            {
                registered = entry.Prefix;
            }
            else
            {
                findings.Add(Warning("MAN-013", file, section.Line, $"No invariant prefix registered for '{module}' in the ownership map."));
            }
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (line, text) in section.Lines.Where(l => l.Text.StartsWith("- ", StringComparison.Ordinal)))
        {
            var match = InvariantIdRegex().Match(text);
            if (!match.Success)
            {
                findings.Add(Error("MAN-012", file, line, "Invariant bullet must start with '**{PFX}-{NN}**'."));
                continue;
            }

            var ruleId = $"{match.Groups[1].Value}-{match.Groups[2].Value}";
            if (!seen.Add(ruleId))
            {
                findings.Add(Error("MAN-012", file, line, $"Duplicate invariant ID '{ruleId}'."));
            }

            if (registered is not null && match.Groups[1].Value != registered)
            {
                findings.Add(Error("MAN-013", file, line, $"Invariant prefix '{match.Groups[1].Value}' must be '{registered}'."));
            }
        }

        if (seen.Count > InvariantsMaxRules)
        {
            findings.Add(Error("MAN-011", file, section.Line, $"Invariants has {seen.Count} rules (max {InvariantsMaxRules}); move detail to /docs/03-Architecture/."));
        }
    }

    // ---- B. Dependencies -------------------------------------------------------------------

    private static async Task LintDependenciesAsync(
        string projectRoot, string moduleDir, ManifestDocument doc, string file, List<ManifestFinding> findings, CancellationToken ct)
    {
        if (!doc.Attributes.TryGetValue("allowed", out var allowedRaw) || !doc.Attributes.TryGetValue("forbidden", out var forbiddenRaw))
        {
            return;
        }

        var csproj = Directory.GetFiles(moduleDir, "*.csproj").FirstOrDefault();
        if (csproj is null)
        {
            findings.Add(Error("MAN-020", file, doc.TagLine, "No .csproj found in module directory."));
            return;
        }

        var allowed = SplitList(allowedRaw);
        var forbidden = SplitList(forbiddenRaw);
        var csprojRel = Relative(projectRoot, csproj);

        await using var stream = File.OpenRead(csproj);
        var xml = await XDocument.LoadAsync(stream, LoadOptions.SetLineInfo, ct).ConfigureAwait(false);

        foreach (var (name, line) in ReadReferences(xml))
        {
            var hit = forbidden.FirstOrDefault(p => GlobMatch(p, name));
            if (hit is not null)
            {
                findings.Add(Error("MAN-022", csprojRel, line, $"Reference '{name}' matches forbidden '{hit}'."));
            }
            else if (!allowed.Any(p => GlobMatch(p, name)))
            {
                findings.Add(Error("MAN-021", csprojRel, line, $"Reference '{name}' is not declared in manifest 'allowed'."));
            }
        }
    }

    private static IEnumerable<(string Name, int Line)> ReadReferences(XDocument xml)
    {
        foreach (var element in xml.Descendants())
        {
            var kind = element.Name.LocalName;
            if (kind is not ("ProjectReference" or "PackageReference"))
            {
                continue;
            }

            var include = element.Attribute("Include")?.Value;
            if (string.IsNullOrWhiteSpace(include))
            {
                continue;
            }

            // Build-time only references (analyzers / source generators) are not runtime dependencies.
            var outputItemType = Value(element, "OutputItemType");
            var referenceOutput = Value(element, "ReferenceOutputAssembly");
            var privateAssets = Value(element, "PrivateAssets");
            if (string.Equals(outputItemType, "Analyzer", StringComparison.OrdinalIgnoreCase)
                || string.Equals(referenceOutput, "false", StringComparison.OrdinalIgnoreCase)
                || string.Equals(privateAssets, "all", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var name = kind == "ProjectReference" ? Path.GetFileNameWithoutExtension(include.Replace('\\', '/')) : include;
            yield return (name, ((System.Xml.IXmlLineInfo)element).LineNumber);
        }

        static string? Value(XElement e, string name) =>
            e.Attribute(name)?.Value ?? e.Elements().FirstOrDefault(c => c.Name.LocalName == name)?.Value;
    }

    // ---- C. Ownership ----------------------------------------------------------------------

    private static void LintMap(OwnershipMap map, string mapRel, HashSet<string> allModules, List<ManifestFinding> findings)
    {
        foreach (var dup in map.Rows.GroupBy(r => r.Id).Where(g => g.Count() > 1))
        {
            findings.Add(Error("MAN-037", mapRel, dup.Last().Line, $"Duplicate ownership row ID '{dup.Key}'."));
        }

        foreach (var row in map.Rows.Where(r => !r.IsRetired))
        {
            if (row.Owner.Equals(OwnershipMap.UnresolvedOwner, StringComparison.Ordinal))
            {
                findings.Add(Error("MAN-036", mapRel, row.Line, $"'{row.Id}' has unresolved owner (TBD)."));
            }
            else if (!row.Owner.Equals(OwnershipMap.HostOwner, StringComparison.Ordinal) && !allModules.Contains(row.Owner))
            {
                findings.Add(Warning("MAN-035", mapRel, row.Line, $"'{row.Id}' owner '{row.Owner}' is not an existing BuildingBlock."));
            }
        }

        foreach (var (module, (prefix, line)) in map.Prefixes)
        {
            if (ReservedPrefixes.Contains(prefix))
            {
                findings.Add(Error("MAN-038", mapRel, line, $"Prefix '{prefix}' for '{module}' collides with a reserved prefix."));
            }
        }

        foreach (var dup in map.Prefixes.GroupBy(p => p.Value.Prefix).Where(g => g.Count() > 1))
        {
            findings.Add(Error("MAN-038", mapRel, dup.Last().Value.Line, $"Prefix '{dup.Key}' is registered for multiple modules."));
        }
    }

    private static HashSet<string> LintOwnership(string module, ManifestDocument doc, string file, OwnershipMap map, List<ManifestFinding> findings)
    {
        var owned = new HashSet<string>(StringComparer.Ordinal);
        var section = FindSection(doc, "Ownership");
        if (section is null)
        {
            return owned;
        }

        var rows = map.Rows.GroupBy(r => r.Id).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        foreach (var (line, text) in section.Lines)
        {
            if (text.Contains("**Owns**", StringComparison.Ordinal))
            {
                foreach (Match m in RowRefRegex().Matches(text))
                {
                    owned.Add(m.Value);
                    if (!rows.TryGetValue(m.Value, out var row))
                    {
                        findings.Add(Error("MAN-031", file, line, $"'{m.Value}' does not exist in the ownership map."));
                    }
                    else if (!row.Owner.Equals(module, StringComparison.OrdinalIgnoreCase))
                    {
                        findings.Add(Error("MAN-032", file, line, $"'{m.Value}' is owned by '{row.Owner}' in the map, not '{module}'."));
                    }
                }
            }
            else if (text.Contains("**Delegates**", StringComparison.Ordinal))
            {
                foreach (Match m in DelegationRegex().Matches(text))
                {
                    var rowId = m.Groups[1].Value;
                    var target = m.Groups[2].Value;
                    if (!rows.TryGetValue(rowId, out var row))
                    {
                        findings.Add(Error("MAN-031", file, line, $"'{rowId}' does not exist in the ownership map."));
                    }
                    else if (!row.Owner.Equals(target, StringComparison.OrdinalIgnoreCase))
                    {
                        findings.Add(Error("MAN-033", file, line, $"'{rowId}' is delegated to '{target}' but the map owner is '{row.Owner}'."));
                    }
                }
            }
        }

        return owned;
    }

    private static void LintCoverage(OwnershipMap map, string mapRel, Dictionary<string, HashSet<string>> claims, List<ManifestFinding> findings)
    {
        foreach (var row in map.Rows.Where(r => !r.IsRetired))
        {
            if (claims.TryGetValue(row.Owner, out var owned) && !owned.Contains(row.Id))
            {
                findings.Add(Error("MAN-034", mapRel, row.Line, $"'{row.Id}' is not claimed in the Owns list of '{row.Owner}'."));
            }
        }
    }

    // ---- Helpers ---------------------------------------------------------------------------

    private static ManifestSection? FindSection(ManifestDocument doc, string title) =>
        doc.Sections.FirstOrDefault(s => s.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

    private static List<string> SplitList(string value) =>
        [.. value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

    private static bool GlobMatch(string pattern, string value) =>
        pattern.EndsWith('*')
            ? value.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase)
            : value.Equals(pattern, StringComparison.OrdinalIgnoreCase);

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static ManifestFinding Error(string code, string file, int line, string message) =>
        new() { Code = code, Severity = ManifestSeverity.Error, File = file, Line = line, Message = message };

    private static ManifestFinding Warning(string code, string file, int line, string message) =>
        new() { Code = code, Severity = ManifestSeverity.Warning, File = file, Line = line, Message = message };

    [GeneratedRegex(@"^(CS|AP|BB|OR|DL|PS)\.\d{2}$")]
    private static partial Regex L3RuleRegex();

    [GeneratedRegex(@"^-\s+\*\*([A-Z]{2,6})-(\d{2,})\*\*")]
    private static partial Regex InvariantIdRegex();

    [GeneratedRegex(@"OWN-[A-Z0-9]+(?:-[A-Z0-9]+)*")]
    private static partial Regex RowRefRegex();

    [GeneratedRegex(@"(OWN-[A-Z0-9]+(?:-[A-Z0-9]+)*)\s*(?:→|->)\s*([A-Za-z0-9.]+)")]
    private static partial Regex DelegationRegex();
}
