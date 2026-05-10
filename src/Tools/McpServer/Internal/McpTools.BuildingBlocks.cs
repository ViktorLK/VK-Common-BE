using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    [McpServerTool]
    [Description("Lists all available BuildingBlock modules and their dependencies. Returns a formatted prompt for the AI.")]
    public static async Task<string> VKListBuildingBlocks(CancellationToken ct)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var blocksDir = Path.Combine(projectRoot, "src", "BuildingBlocks");

            if (!Directory.Exists(blocksDir))
            {
                return $"[Error] BuildingBlocks directory not found at: {blocksDir}";
            }

            var modules = new List<object>();
            var directories = Directory.GetDirectories(blocksDir);

            foreach (var dir in directories)
            {
                var moduleName = Path.GetFileName(dir);
                var csprojFile = Directory.GetFiles(dir, "*.csproj").FirstOrDefault();

                if (csprojFile != null)
                {
                    var dependencies = await GetDependenciesAsync(csprojFile, ct).ConfigureAwait(false);
                    modules.Add(new
                    {
                        module = moduleName,
                        project = Path.GetFileName(csprojFile),
                        dependencies = dependencies
                    });
                }
            }

            var json = JsonSerializer.Serialize(modules, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return $"[MCP Tool: VKListBuildingBlocks]\nHere are the currently available BuildingBlocks in the solution:\n{json}";
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to list BuildingBlocks: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Generates standard VK.Blocks boilerplate for a new building block library. Returns a prompt with file content and instructions.")]
    public static async Task<string> VKDraftBuildingBlockBoilerplate(
        [Description("The name of the building block (e.g. 'Logging', 'Caching').")] string blockName,
        [Description("The category of the block (e.g. 'Web', 'Infrastructure'). Used for configuration path.")] string? category = null,
        [Description("List of block identifiers this block depends on. Defaults to ['Core'].")] string[]? dependencies = null,
        CancellationToken ct = default)
    {
        try
        {
            var blockIdentifier = blockName;
            var deps = dependencies ?? new[] { "Core" };
            var depsList = string.Join(", ", deps.Select(d => $"typeof(VK{d}Block)"));
            var configPath = !string.IsNullOrEmpty(category) ? $"{category}:{blockName}" : blockName;

            var prompt = $@"
[MCP Tool: VKDraftBuildingBlockBoilerplate]
You are acting as a senior .NET framework architect.

Your task is to initialize the `{blockName}` BuildingBlock following the **VK.Blocks Industrial DNA (v2)** standards.
Execute ALL steps below to create a consistent, industrial-grade library.

### STEP 1: Directory Structure
Create the following layout in `src/BuildingBlocks/{blockName}/`:
- `Abstractions/`
- `Contracts/`
- `DependencyInjection/Internal/`
- `Diagnostics/Internal/`
- `Features/Internal/`

### STEP 2: The Block Marker (The Heart)
File: `src/BuildingBlocks/{blockName}/VK{blockIdentifier}Block.cs`
```csharp
using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.{blockIdentifier};

/// <summary>
/// A marker type for the VK.Blocks.{blockIdentifier} building block.
/// </summary>
[ExcludeFromCodeCoverage]
[VKBlockMarker(Dependencies = [{depsList}])]
public sealed partial class VK{blockIdentifier}Block;
```

### STEP 3: Configuration Options (Immutable Record)
File: `src/BuildingBlocks/{blockName}/DependencyInjection/VK{blockIdentifier}Options.cs`
```csharp
using VK.Blocks.Core;

namespace VK.Blocks.{blockIdentifier};

/// <summary>
/// Configuration options for the {blockIdentifier} building block.
/// </summary>
public sealed record VK{blockIdentifier}Options : IVKBlockOptions
{{
    /// <summary>
    /// The configuration section name for {blockIdentifier} options.
    /// </summary>
    public static string SectionName => $""{{VKBlocksConstants.VKBlocksConfigPrefix}}:{configPath}"";

    /// <summary>
    /// Gets a value indicating whether the {blockIdentifier} block is enabled.
    /// </summary>
    public bool Enabled {{ get; init; }} = false;
}}
```

### STEP 4: Public Entry Point (Fluent API)
File: `src/BuildingBlocks/{blockName}/DependencyInjection/VK{blockIdentifier}BlockExtensions.cs`
```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.{blockIdentifier}.DependencyInjection.Internal;

namespace VK.Blocks.{blockIdentifier};

public static class VK{blockIdentifier}BlockExtensions
{{
    /// <summary>
    /// Adds the {blockIdentifier} building block to the service collection.
    /// </summary>
    public static IVK{blockIdentifier}Builder Add{blockIdentifier}Block(
        this IServiceCollection services, 
        IConfiguration configuration,
        Func<VK{blockIdentifier}Options, VK{blockIdentifier}Options>? configure = null)
        => {blockIdentifier}BlockRegistration.Register(services, configuration, configure);
}}
```

### STEP 5: Registration Logic (Industrial DNA Registration Loop)
File: `src/BuildingBlocks/{blockName}/DependencyInjection/Internal/{blockIdentifier}BlockRegistration.cs`
```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.{blockIdentifier}.DependencyInjection.Internal;

/// <summary>
/// Internal registration logic for the {blockIdentifier} block.
/// </summary>
internal static class {blockIdentifier}BlockRegistration
{{
    internal static IVK{blockIdentifier}Builder Register(
        IServiceCollection services, 
        IConfiguration configuration,
        Func<VK{blockIdentifier}Options, VK{blockIdentifier}Options>? configure = null)
    {{
        // 1. Check-Self & Prerequisite Check
        // AP.02: This handles both self-idempotency and recursive dependency validation.
        if (services.IsVKBlockRegistered<VK{blockIdentifier}Block>())
        {{
            return new {blockIdentifier}BlockBuilder(services, configuration);
        }}

        // 2. Options Registration
        // AP.04: Bind options before marker registration.
        // Rule 20: Use functional transformation to support immutable options.
        VK{blockIdentifier}Options options = services.AddVKBlockOptions<VK{blockIdentifier}Options>(configuration, configure);

        // 3. Success Commit (Marker)
        // AP.02: Register marker immediately after options but before feature-gate early return.
        services.AddVKBlockMarker<VK{blockIdentifier}Block>();

        // 4. Options Validation
        // Mandatory for config safety.
        services.TryAddEnumerableSingleton<IValidateOptions<VK{blockIdentifier}Options>, {blockIdentifier}OptionsValidator>();

        // 5. Diagnostics & Metadata
        // [Add diagnostics/metadata registration here]

        // 6. Early Return (Feature Toggle)
        // AP.02: Enabled check AFTER marker.
        if (!options.Enabled)
        {{
            return new {blockIdentifier}BlockBuilder(services, configuration);
        }}

        // 7. Core Services
        // [Implement internal services here]

        return new {blockIdentifier}BlockBuilder(services, configuration);
    }}
}}
```

### STEP 6: Diagnostics Boilerplate
File: `src/BuildingBlocks/{blockName}/Diagnostics/Internal/{blockIdentifier}Diagnostics.cs`
```csharp
using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.{blockIdentifier}.Diagnostics.Internal;

/// <summary>
/// Centralized Diagnostics definition for the VK.Blocks.{blockIdentifier} building block.
/// </summary>
[VKBlockDiagnostics<VK{blockIdentifier}Block>]
internal static partial class {blockIdentifier}Diagnostics
{{
    // ActivitySource and Meter are generated automatically into a partial class.
}}
```

### STEP 7: Builder & Support
Create `IVK{blockIdentifier}Builder.cs` in `DependencyInjection/` and `{blockIdentifier}BlockBuilder.cs`, `{blockIdentifier}OptionsValidator.cs` in `Internal/`.

Follow BB.01-BB.05 strictly. Do NOT skip any file or step. All classes must be `sealed`. Use `VKGuard` at boundaries.
";
            return prompt.Trim();
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to generate boilerplate: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Retrieves localized prompts by scanning recursively from the target path up to the src/ directory. Supports cascading inheritance of instructions and automated flattening of rules.")]
    public static async Task<string> VKGetModuleContext(
        [Description("The target path to start scanning from (e.g. 'src/BuildingBlocks/Core').")] string path,
        CancellationToken ct)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var absoluteTarget = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(projectRoot, path));
            var srcPath = Path.GetFullPath(Path.Combine(projectRoot, "src"));

            var currentDir = absoluteTarget;
            var promptFiles = new List<string>();

            // 1. Collect prompt files from child to parent
            while (!string.IsNullOrEmpty(currentDir) && currentDir.Length >= srcPath.Length && currentDir.StartsWith(srcPath, StringComparison.OrdinalIgnoreCase))
            {
                var promptsDir = Path.Combine(currentDir, ".prompts");
                if (Directory.Exists(promptsDir))
                {
                    promptFiles.AddRange(Directory.GetFiles(promptsDir, "*.md"));
                }

                var parent = Path.GetDirectoryName(currentDir);
                if (parent == currentDir)
                    break;
                currentDir = parent;
            }

            if (promptFiles.Count == 0)
            {
                return $"[Info] No local prompts found in the path hierarchy of '{path}'. Proceed with global rules.";
            }

            // 2. Reverse to maintain hierarchy (Parent -> Child)
            promptFiles.Reverse();

            // 3. Flattening Engine
            var activeRules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var metadata = new List<string>();

            foreach (var file in promptFiles)
            {
                var content = await File.ReadAllTextAsync(file, ct).ConfigureAwait(false);
                var (yaml, body) = ParseMarkdownFile(content);

                // Handle 'requires' - fetch global rules
                if (yaml.TryGetValue("requires", out var requiresStr))
                {
                    var ruleIds = ResolveRuleRange(requiresStr);
                    foreach (var id in ruleIds)
                    {
                        var ruleContent = await GetRuleContentInternal(id, ct).ConfigureAwait(false);
                        if (ruleContent != null)
                        {
                            activeRules[id] = ruleContent;
                        }
                    }
                }

                // Handle 'overrides' - remove specific rules if explicitly mentioned
                if (yaml.TryGetValue("overrides", out var overridesStr))
                {
                    var ruleIds = ResolveRuleRange(overridesStr);
                    foreach (var id in ruleIds)
                    {
                        activeRules.Remove(id);
                    }
                }

                // Parse sections (## ID or ### ID) and merge
                var sections = SplitIntoSections(body);
                foreach (var section in sections)
                {
                    activeRules[section.Key] = section.Value;
                }

                metadata.Add($"Merged layer: {Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(file)))}/{Path.GetFileName(file)}");
            }

            // 4. Build Final Flattened Prompt
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("# 🏛️ Flattened Architectural Context (Single Source of Truth)");
            sb.AppendLine($"> Target: {path}");
            sb.AppendLine($"> Layers: {string.Join(" -> ", metadata)}");
            sb.AppendLine("\n---");
            sb.AppendLine("\n## 📜 Active Rule Set\n");

            foreach (var rule in activeRules.OrderBy(r => r.Key))
            {
                var content = rule.Value.TrimEnd('-').Trim();
                sb.AppendLine(content);
                sb.AppendLine("\n---");
            }

            sb.AppendLine("\n## 🛡️ Audit Protocol");
            sb.AppendLine("Follow the L3 Audit checklist in `vk-blocks-checklist.md` using the rule evidence from the flattened context above.");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to flatten module context: {ex.Message}";
        }
    }

    private static (Dictionary<string, string> Yaml, string Body) ParseMarkdownFile(string content)
    {
        var yaml = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var body = content;

        if (content.StartsWith("---"))
        {
            var endOfYaml = content.IndexOf("---", 3);
            if (endOfYaml != -1)
            {
                var yamlContent = content.Substring(3, endOfYaml - 3);
                body = content.Substring(endOfYaml + 3).Trim();

                var lines = yamlContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();
                        if (yaml.ContainsKey(key))
                        {
                            yaml[key] = $"{yaml[key]}, {value}";
                        }
                        else
                        {
                            yaml[key] = value;
                        }
                    }
                }
            }
        }

        return (yaml, body);
    }

    private static readonly string[] MasterRuleIds =
    [
        "CS.01", "CS.02", "CS.03", "CS.04", "CS.05", "CS.06",
        "OR.01", "OR.02", "OR.03",
        "DL.01", "DL.02", "DL.03", "DL.04",
        "AP.01", "AP.02", "AP.03", "AP.04", "AP.05",
        "BB.01", "BB.02", "BB.03", "BB.04", "BB.05",
        "PS.01", "PS.02", "PS.03", "PS.04"
    ];

    private static List<string> ResolveRuleRange(string rangeStr)
    {
        var result = new List<string>();
        var parts = rangeStr.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var cleanPart = part.Trim();
            if (cleanPart.Contains('-'))
            {
                var rangeParts = cleanPart.Split('-');
                if (rangeParts.Length == 2)
                {
                    var start = rangeParts[0].Trim();
                    var end = rangeParts[1].Trim();

                    var startIndex = Array.FindIndex(MasterRuleIds, id => id.Equals(start, StringComparison.OrdinalIgnoreCase));
                    var endIndex = Array.FindIndex(MasterRuleIds, id => id.Equals(end, StringComparison.OrdinalIgnoreCase));

                    if (startIndex != -1 && endIndex != -1 && startIndex <= endIndex)
                    {
                        for (int i = startIndex; i <= endIndex; i++)
                        {
                            result.Add(MasterRuleIds[i]);
                        }
                    }
                    else
                    {
                        // Fallback: just add start and end
                        result.Add(start);
                        result.Add(end);
                    }
                }
            }
            else
            {
                result.Add(cleanPart);
            }
        }
        return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static Dictionary<string, string> SplitIntoSections(string body)
    {
        var sections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // Match headers like ## ID: Name [tags] or ### ID
        var regex = new Regex(@"^(#{2,3})\s+([A-Z0-9.]+)(:.*)?$", RegexOptions.Multiline);
        var matches = regex.Matches(body);

        for (int i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            var id = match.Groups[2].Value.Trim();
            var start = match.Index;
            var end = (i + 1 < matches.Count) ? matches[i + 1].Index : body.Length;

            sections[id] = body.Substring(start, end - start).Trim();
        }

        return sections;
    }

    private static async Task<List<string>> GetDependenciesAsync(string csprojPath, CancellationToken ct)
    {
        var dependencies = new List<string>();
        try
        {
            var content = await File.ReadAllTextAsync(csprojPath, ct).ConfigureAwait(false);
            // Simple regex for ProjectReference
            var regex = new Regex(@"<ProjectReference\s+Include=""([^""]+)""");
            var matches = regex.Matches(content);

            foreach (Match match in matches)
            {
                var depPath = match.Groups[1].Value;
                var depName = Path.GetFileNameWithoutExtension(depPath);
                dependencies.Add(depName);
            }
        }
        catch
        {
            // Ignore read errors
        }
        return dependencies;
    }
}

