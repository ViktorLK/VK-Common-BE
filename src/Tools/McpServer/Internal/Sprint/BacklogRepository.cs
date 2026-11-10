using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VK.Tools.McpServer.Internal.Backlogs;

internal static partial class BacklogRepository
{
    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public static string GetBacklogsDir(string projectRoot) => Path.Combine(projectRoot, "docs", "07-Backlogs");
    public static string GetSprintsDir(string projectRoot) => Path.Combine(GetBacklogsDir(projectRoot), "_sprints");

    private static readonly FrozenDictionary<string, string> ValidPillarModules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Core"] = "Core",
        ["AI"] = "AI",
        ["Authentication"] = "Authentication",
        ["Authorization"] = "Authorization",
        ["Persistence"] = "Persistence",
        ["Validation"] = "Validation",
        ["VectorStore"] = "VectorStore",
        ["VectorSearch"] = "VectorSearch",
        ["MultiTenancy"] = "MultiTenancy",
        ["Observability"] = "Observability",
        ["Security"] = "Security",
        ["Web"] = "Web",
        ["Tools"] = "Tools",
        ["Labs"] = "Labs"
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static string ResolvePillarModule(string module)
    {
        var trimmed = module.Trim();
        if (ValidPillarModules.TryGetValue(trimmed, out var canonical))
        {
            return canonical;
        }

        return trimmed;
    }

    public static string SerializeWithFrontMatter<T>(T frontMatter, string body)
    {
        var yaml = YamlSerializer.Serialize(frontMatter);
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.Append(yaml);
        if (!yaml.EndsWith("\n", StringComparison.Ordinal)) sb.AppendLine();
        sb.AppendLine("---");
        sb.Append(body.TrimStart('\r', '\n'));
        return sb.ToString();
    }

    public static bool TryParseFrontMatter<T>(string fileContent, out T? frontMatter, out string body) where T : class
        => TryParseFrontMatter(fileContent, out frontMatter, out body, out _);

    public static bool TryParseFrontMatter<T>(string fileContent, out T? frontMatter, out string body, out string? error) where T : class
    {
        frontMatter = null;
        body = fileContent;
        error = null;

        if (!fileContent.StartsWith("---", StringComparison.Ordinal))
        {
            error = "File does not start with front-matter delimiter '---'.";
            return false;
        }

        var secondIndex = fileContent.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (secondIndex == -1)
        {
            secondIndex = fileContent.IndexOf("\r\n---", 3, StringComparison.Ordinal);
            if (secondIndex == -1)
            {
                error = "Closing front-matter delimiter '---' not found.";
                return false;
            }
        }

        var headerEnd = fileContent.IndexOf('\n', secondIndex + 1);
        if (headerEnd == -1) headerEnd = fileContent.Length;

        var yamlContent = fileContent.Substring(3, secondIndex - 3).Trim();
        body = fileContent.Substring(headerEnd).TrimStart('\r', '\n');

        try
        {
            frontMatter = YamlDeserializer.Deserialize<T>(yamlContent);
            if (frontMatter == null)
            {
                error = "Deserialized front-matter object was null.";
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            error = $"YAML deserialization failed: {ex.Message}";
            return false;
        }
    }

    public static async Task<List<BacklogItemModel>> LoadAllBacklogsAsync(string projectRoot, CancellationToken ct)
    {
        var backlogsDir = GetBacklogsDir(projectRoot);
        var result = new List<BacklogItemModel>();
        if (!Directory.Exists(backlogsDir)) return result;

        var files = Directory.GetFiles(backlogsDir, "*.md", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            if (fileName.Equals("Active_Sprint.md", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("Product_Backlog.md", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("Health_Scorecard.md", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("README.md", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (file.Contains(Path.Combine("07-Backlogs", "_sprints"), StringComparison.OrdinalIgnoreCase) || file.Contains(Path.Combine("07-Backlogs", "Sprints"), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var text = await File.ReadAllTextAsync(file, ct).ConfigureAwait(false);

            if (TryParseFrontMatter<BacklogFrontMatter>(text, out var fm, out var body) && fm != null)
            {
                result.Add(new BacklogItemModel
                {
                    FrontMatter = fm,
                    BodyMarkdown = body,
                    FilePath = file,
                    IsArchived = fm.Status == "done" || fm.Status == "dropped"
                });
            }
        }

        return result;
    }

    public static async Task<int> GetNextGlobalIndexAsync(string projectRoot, CancellationToken ct = default)
    {
        // R1 Fix: strictly evaluate front-matter 'id' from loaded backlog items only.
        var items = await LoadAllBacklogsAsync(projectRoot, ct).ConfigureAwait(false);
        int max = 0;
        foreach (var item in items)
        {
            var rawId = item.FrontMatter.Id?.Trim() ?? string.Empty;
            var match = Regex.Match(rawId, @"^#(\d+)$");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var n))
            {
                if (n > max) max = n;
            }
        }

        return max == 0 ? 1 : max + 1;
    }

    public static string NormalizeSlug(string title)
    {
        var s = title.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"[^\w\s-]", "");
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, @"-+", "-");
        return s.Trim('-');
    }

    public static (List<string> Commits, string? Error) FindGitCommitsForTask(string projectRoot, string taskId)
    {
        var commits = new List<string>();
        try
        {
            var cleanId = taskId.StartsWith("#", StringComparison.Ordinal) ? taskId : $"#{taskId}";
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                WorkingDirectory = projectRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            psi.ArgumentList.Add("log");
            psi.ArgumentList.Add("--all");
            psi.ArgumentList.Add("--fixed-strings");
            psi.ArgumentList.Add($"--grep=[{cleanId}]");
            psi.ArgumentList.Add("--format=%h %s");

            using var process = Process.Start(psi);
            if (process == null)
            {
                return (commits, "Failed to start git process.");
            }

            var output = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(3000))
            {
                try { process.Kill(); } catch (InvalidOperationException) { }
                return (commits, "git log execution timed out after 3000ms.");
            }

            if (process.ExitCode != 0)
            {
                return (commits, $"git log failed with code {process.ExitCode}: {stderr.Trim()}");
            }

            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            commits.AddRange(lines);
            return (commits, null);
        }
        catch (Exception ex)
        {
            return (commits, ex.Message);
        }
    }

    public static string GenerateDoDForCategory(string category, string title) => category.ToLowerInvariant().Trim() switch
    {
        "spike" => $"""
## ✅ DoD (Definition of Done)
- [ ] {title}
- [ ] 調査結論を `docs/` に技術文書またはスパイクレポートとして記録
- [ ] 後続のアクション・実装 PBI を起票（必要に応じて `vk-add-backlog` で起票）
- [ ] **ADR または RFC 要件判定 (`DL.03`, `PS.03`)**
""",
        "feature" => $"""
## ✅ DoD (Definition of Done)
- [ ] {title}
- [ ] 子 Story / Task に分解完了（各子 PBI の parent に本 ID を指定）
- [ ] 承認済み RFC ドキュメント（`docs/02-RFCs/`）の紐付け確認
""",
        _ => $"""
## ✅ DoD (Definition of Done)
- [ ] {title}
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み
"""
    };

    public static (bool IsReady, List<string> Deficiencies) EvaluateDoR(BacklogFrontMatter fm, string bodyMarkdown)
    {
        var defs = new List<string>();

        // 1. Points assigned and <= 13
        if (!fm.Points.HasValue)
        {
            defs.Add("Story Points (points) is not set.");
        }
        else if (fm.Points.Value <= 0 || !BacklogConstants.ValidFibonacciPoints.Contains(fm.Points.Value))
        {
            defs.Add($"Story Points must be a valid Fibonacci value (1, 2, 3, 5, 8, 13). Current: {fm.Points.Value}.");
        }

        // 2. Problem/Context and Proposed Changes in Description
        if (!bodyMarkdown.Contains("### 1. Problem / Context", StringComparison.OrdinalIgnoreCase))
        {
            defs.Add("Missing '### 1. Problem / Context' section in Description.");
        }
        if (!bodyMarkdown.Contains("### 2. Proposed Changes", StringComparison.OrdinalIgnoreCase))
        {
            defs.Add("Missing '### 2. Proposed Changes' section in Description.");
        }

        // 3. DoD contains at least one task checkbox besides title
        var matches = Regex.Matches(bodyMarkdown, @"^-\s*\[\s*[ xX]\s*\]\s*(.+)$", RegexOptions.Multiline);
        if (matches.Count < 2)
        {
            defs.Add("DoD must contain at least one specific verification checklist item besides the title.");
        }

        // 4. Category cannot be feature (must be decomposed first)
        if (string.Equals(fm.Category, "feature", StringComparison.OrdinalIgnoreCase))
        {
            defs.Add("Category 'feature' cannot be marked Ready. Decompose into atomic stories/tasks first.");
        }

        return (defs.Count == 0, defs);
    }

    public static async Task<int?> CalculateAverageVelocityAsync(string projectRoot, CancellationToken ct)
    {
        var sprintsDir = GetSprintsDir(projectRoot);
        if (!Directory.Exists(sprintsDir)) return null;

        var files = Directory.GetFiles(sprintsDir, "*.md");
        var closedSprints = new List<SprintFrontMatter>();

        foreach (var file in files)
        {
            var text = await File.ReadAllTextAsync(file, ct).ConfigureAwait(false);
            if (TryParseFrontMatter<SprintFrontMatter>(text, out var sfm, out _) && sfm != null)
            {
                if (string.Equals(sfm.Status, "closed", StringComparison.OrdinalIgnoreCase) && sfm.CompletedPoints.HasValue)
                {
                    closedSprints.Add(sfm);
                }
            }
        }

        if (closedSprints.Count == 0) return null;

        var recent = closedSprints
            .OrderByDescending(s => s.End)
            .Take(3)
            .ToList();

        var avg = recent.Average(s => s.CompletedPoints!.Value);
        return (int)Math.Round(avg);
    }
}
