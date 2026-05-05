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
            var depsArray = string.Join(", ", deps.Select(d => $"{d}Block.Instance"));
            var configPath = !string.IsNullOrEmpty(category) ? $"{category}:{blockName}" : blockName;

            var prompt = $@"
[MCP Tool: VKDraftBuildingBlockBoilerplate]
You are acting as a senior .NET framework architect.

Your task is to initialize the `{blockName}` BuildingBlock following the **VK.Blocks Blueprint**.
Execute ALL steps below to create a consistent, industrial-grade library.

### STEP 1: Directory Structure
Create the following layout in `src/BuildingBlocks/{blockName}/`:
- `Abstractions/`
- `Contracts/`
- `DependencyInjection/Internal/`
- `Diagnostics/Internal/`
- `Features/Internal/`

### STEP 2: The Marker (Contract)
File: `src/BuildingBlocks/{blockName}/Contracts/{blockIdentifier}Block.cs`
```csharp
using VK.Blocks.Core.Constants;
using VK.Blocks.Core.Contracts;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.{blockIdentifier}.Contracts;

public sealed partial class {blockIdentifier}Block : IVKBlockMarker
{{
    private {blockIdentifier}Block() {{ }}
    public static {blockIdentifier}Block Instance {{ get; }} = new();

    public string Identifier => ""{blockIdentifier}"";
    public string Version => ""1.0.0"";
    public IReadOnlyList<IVKBlockMarker> Dependencies => [{depsArray}];
    public string ActivitySourceName => VKBlocksConstants.VKBlocksPrefix + Identifier;
    public string MeterName => VKBlocksConstants.VKBlocksPrefix + Identifier;
}}
```

### STEP 3: Configuration Options
File: `src/BuildingBlocks/{blockName}/DependencyInjection/VK{blockIdentifier}Options.cs`
```csharp
using VK.Blocks.Core.Constants;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.{blockIdentifier};

public sealed record VK{blockIdentifier}Options : IVKBlockOptions
{{
    public static string SectionName => VKBlocksConstants.VKBlocksConfigPrefix + ""{configPath}"";
    public bool Enabled {{ get; init; }} = false;
}}
```

### STEP 4: Public Entry Point (Wrapper)
File: `src/BuildingBlocks/{blockName}/DependencyInjection/VK{blockIdentifier}BlockExtensions.cs`
```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.{blockIdentifier}.DependencyInjection.Internal;

namespace VK.Blocks.{blockIdentifier};

public static class VK{blockIdentifier}BlockExtensions
{{
    public static IVK{blockIdentifier}Builder Add{blockIdentifier}Block(this IServiceCollection services, IConfiguration configuration)
        => {blockIdentifier}BlockRegistration.Register(services, configuration);
}}
```

### STEP 5: Registration Logic (Internal Core)
File: `src/BuildingBlocks/{blockName}/DependencyInjection/Internal/{blockIdentifier}BlockRegistration.cs`
```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.{blockIdentifier}.Contracts;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.{blockIdentifier}.DependencyInjection.Internal;

internal static class {blockIdentifier}BlockRegistration
{{
    internal static IVK{blockIdentifier}Builder Register(IServiceCollection services, IConfiguration configuration)
    {{
        // 1. Check-Self
        if (services.IsVKBlockRegistered<{blockIdentifier}Block>())
        {{
            return new {blockIdentifier}BlockBuilder(services, configuration);
        }}

        // 2. Check-Prerequisite
        services.EnsureVKCoreBlockRegistered<{blockIdentifier}Block>();

        // 3. Options Registration
        VK{blockIdentifier}Options options = services.AddVKBlockOptions<VK{blockIdentifier}Options>(configuration);

        // 4. Mark-Self (Crucial: Before feature gate)
        services.AddVKBlockMarker<{blockIdentifier}Block>();

        // 5. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VK{blockIdentifier}Options>, {blockIdentifier}OptionsValidator>();

        // 6. Feature Toggle
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

### STEP 6: Support Classes (Internal)
Create `{blockIdentifier}BlockBuilder.cs` and `{blockIdentifier}OptionsValidator.cs` in the `Internal/` folder.

Follow Rule 16-20 strictly. Do NOT skip any file or step.
";
            return prompt.Trim();
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to generate boilerplate: {ex.Message}";
        }
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
