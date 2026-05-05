using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using VK.Tools.McpServer.Internal;

namespace VK.Tools.McpServer;

/// <summary>
/// Entry point for the VK.Blocks MCP Server.
/// </summary>
internal static class Program
{
    /* 
     * [Developer Note]
     * For hot-reloading during development, you can use 'dotnet watch run' in mcp_config.json:
     * "command": "dotnet",
     * "args": ["watch", "run", "--quiet", "--non-interactive", "--project", ".../VK.Tools.McpServer.csproj"]
     * Note: Ensure no non-JSON text is output to stdout by watch/run.
     */
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Disable default logging to stdout as it breaks the MCP protocol
        builder.Logging.ClearProviders();

        // Configure MCP Server
        builder.Services.AddMcpServer(options =>
        {
            options.ServerInfo = new Implementation
            {
                Name = "vk-blocks-manager",
                Version = "1.0.0"
            };
        })
        .WithStdioServerTransport()
        .WithToolsFromAssembly();

        // Register Tool Services
        builder.Services.AddSingleton<McpTools>();

        using var host = builder.Build();

        await host.RunAsync().ConfigureAwait(false);
    }
}
