using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    [McpServerTool]
    [Description("Parses an OpenAPI JSON file and returns a prompt instructing the AI to generate integration tests.")]
    public static async Task<string> VKDraftApiIntegrationTests(
        [Description("Path to the swagger.json file.")] string swaggerJsonPath,
        CancellationToken ct)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var absPath = Path.IsPathRooted(swaggerJsonPath) ? swaggerJsonPath : Path.Combine(projectRoot, swaggerJsonPath);

            if (!File.Exists(absPath))
            {
                return $"[Error] Swagger file not found at: {absPath}";
            }

            var content = await File.ReadAllTextAsync(absPath, ct).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(content);

            if (!doc.RootElement.TryGetProperty("paths", out var paths))
            {
                return "[Error] Invalid OpenAPI JSON: 'paths' object not found.";
            }

            var endpoints = new List<string>();

            foreach (var pathProperty in paths.EnumerateObject())
            {
                var apiPath = pathProperty.Name;
                foreach (var methodProperty in pathProperty.Value.EnumerateObject())
                {
                    var method = methodProperty.Name.ToUpperInvariant();
                    var summary = "No summary";
                    if (methodProperty.Value.TryGetProperty("summary", out var summaryProp))
                    {
                        summary = summaryProp.GetString();
                    }
                    endpoints.Add($"- [{method}] {apiPath} ({summary})");
                }
            }

            var prompt = $@"
[MCP Tool: draft_api_tests]
I have parsed the OpenAPI specification from `{swaggerJsonPath}`.
Found {endpoints.Count} endpoints.

Endpoints:
{string.Join("\n", endpoints)}

Your task:
Generate C# Integration Tests (using WebApplicationFactory and xUnit) for the endpoints listed above.
Ensure you test:
1. Happy Path (200 OK or 201 Created)
2. Validation Failure (400 Bad Request)
3. Unauthorized/Forbidden (401/403)
";
            return prompt.Trim();
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to parse Swagger file: {ex.Message}";
        }
    }
}
