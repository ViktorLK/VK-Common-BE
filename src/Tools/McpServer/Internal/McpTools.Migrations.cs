using System;
using System.Collections.Generic;
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
    [Description("Scans EF Core migration files for destructive operations. Returns a prompt instructing the AI to warn the user.")]
    public static async Task<string> VKAuditMigrations(
        [Description("Path to the EF Core Migrations directory (e.g. 'src/BuildingBlocks/Identity/Migrations').")] string migrationsDir,
        CancellationToken ct)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var absPath = Path.IsPathRooted(migrationsDir) ? migrationsDir : Path.Combine(projectRoot, migrationsDir);

            if (!Directory.Exists(absPath))
            {
                return $"[Error] Migrations directory not found at: {absPath}";
            }

            var files = Directory.GetFiles(absPath, "*.cs");
            var migrationFiles = files.Where(f => !f.EndsWith(".Designer.cs") && !f.EndsWith("ModelSnapshot.cs")).ToList();

            var destructivePatterns = new[]
            {
                "migrationBuilder.DropTable",
                "migrationBuilder.DropColumn",
                "migrationBuilder.DropForeignKey",
                "migrationBuilder.DropIndex",
                "migrationBuilder.Sql(\"DROP",
                "migrationBuilder.Sql(\"DELETE",
                "migrationBuilder.Sql(\"TRUNCATE"
            };

            var findings = new List<string>();

            foreach (var file in migrationFiles)
            {
                var content = await File.ReadAllLinesAsync(file, ct).ConfigureAwait(false);
                for (int i = 0; i < content.Length; i++)
                {
                    var line = content[i];
                    if (destructivePatterns.Any(p => line.Contains(p, StringComparison.OrdinalIgnoreCase)))
                    {
                        findings.Add($"- {Path.GetFileName(file)}:{i + 1} -> `{line.Trim()}`");
                    }
                }
            }

            if (findings.Count > 0)
            {
                return $"[MCP Tool: audit_ef_migrations]\nI have scanned the migrations in `{migrationsDir}`.\n\nCRITICAL WARNING: Found {findings.Count} potentially destructive operations:\n{string.Join("\n", findings)}\n\nYour task: WARN the user about these destructive changes and ask for explicit confirmation before they apply this migration to production. Explain the impact of each finding.";
            }

            return "[MCP Tool: audit_ef_migrations]\nSUCCESS: No obvious destructive operations (DROP TABLE, DROP COLUMN) were found in the C# migration files. It appears safe.";
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to audit migrations: {ex.Message}";
        }
    }
}
