using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    [McpServerTool]
    [Description("Consolidates source code from a directory into a single Markdown file. Supports path-specific exports and proper encoding.")]
    public static async Task<string> VKExportCodebaseAsMarkdown(
        [Description("Directory to scan (relative to project root, e.g. 'src/BuildingBlocks/Core').")] string sourcePath = "src",
        [Description("Output Markdown file name (relative to project root).")] string outputPath = "CodebaseSnapshot.md",
        [Description("Optional snapshot ID to group multiple exports.")] string? snapshotId = null,
        CancellationToken ct = default)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var fullSourcePath = Path.IsPathRooted(sourcePath) ? sourcePath : Path.Combine(projectRoot, sourcePath);

            // Generate timestamp: yyyyMMddHHmmss
            var now = DateTime.Now;
            var timestamp = now.ToString("yyyyMMddHHmmss");

            // Ensure relative output paths are placed in artifacts/Snapshots_{timestamp}
            var finalPath = outputPath;
            if (!Path.IsPathRooted(outputPath))
            {
                // Strip "artifacts/" prefix if already present to avoid nesting
                var normalizedPath = outputPath.Replace('\\', '/');
                if (normalizedPath.StartsWith("artifacts/", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedPath = normalizedPath["artifacts/".Length..];
                }

                if (normalizedPath.StartsWith("Snapshots_", StringComparison.OrdinalIgnoreCase))
                {
                    finalPath = Path.Combine("artifacts", normalizedPath);
                }
                else
                {
                    var folderName = string.IsNullOrWhiteSpace(snapshotId)
                        ? $"Snapshots_{timestamp}"
                        : (snapshotId.StartsWith("Snapshots_") ? snapshotId : $"Snapshots_{snapshotId}");

                    finalPath = Path.Combine("artifacts", folderName, normalizedPath);
                }
            }

            var fullOutputPath = Path.IsPathRooted(finalPath) ? finalPath : Path.Combine(projectRoot, finalPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath)!);

            var excludePatterns = new[] { "bin", "obj", ".git", ".vs", "node_modules", "artifacts", "TestResults", ".gemini", ".nx" };
            var allowedExtensions = new[] { ".cs", ".csproj", ".json", ".md", ".yaml", ".yml", ".ts", ".js" };

            var files = GetFilesRecursively(fullSourcePath, excludePatterns, allowedExtensions);

            var sb = new StringBuilder();
            sb.Append($"# Codebase Snapshot\nGenerated on: {DateTime.Now:O}\nSource Path: {sourcePath}\n\n---\n\n");

            var fileContents = new string[files.Count];

            await Parallel.ForEachAsync(Enumerable.Range(0, files.Count), new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = Environment.ProcessorCount }, async (i, token) =>
            {
                var file = files[i];
                var relativePath = Path.GetRelativePath(projectRoot, file);
                var content = await File.ReadAllTextAsync(file, token).ConfigureAwait(false);
                var ext = Path.GetExtension(file).ToLowerInvariant();
                var lang = ext switch
                {
                    ".cs" => "csharp",
                    ".csproj" => "xml",
                    ".md" => "markdown",
                    ".json" => "json",
                    ".yaml" or ".yml" => "yaml",
                    _ => ""
                };

                fileContents[i] = $"## File: {relativePath}\n```{lang}\n{content}\n```\n\n---\n\n";
            }).ConfigureAwait(false);

            foreach (var chunk in fileContents)
            {
                sb.Append(chunk);
            }

            await File.WriteAllTextAsync(fullOutputPath, sb.ToString(), ct).ConfigureAwait(false);

            return $"Successfully exported {files.Count} files from `{sourcePath}` to `{finalPath}`.";
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to export codebase: {ex.Message}";
        }
    }

    private static List<string> GetFilesRecursively(string root, string[] excludePatterns, string[] allowedExtensions)
    {
        var result = new List<string>();
        if (!Directory.Exists(root))
            return result;

        foreach (var file in Directory.EnumerateFiles(root))
        {
            if (allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
            {
                result.Add(file);
            }
        }

        foreach (var dir in Directory.EnumerateDirectories(root))
        {
            var dirName = Path.GetFileName(dir);
            if (excludePatterns.Contains(dirName, StringComparer.OrdinalIgnoreCase))
                continue;

            result.AddRange(GetFilesRecursively(dir, excludePatterns, allowedExtensions));
        }

        return result;
    }
}
