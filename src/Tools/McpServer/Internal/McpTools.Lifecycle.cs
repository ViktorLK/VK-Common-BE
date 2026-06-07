using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    [McpServerTool]
    [Description("Shuts down the server to allow rebuilding (Development mode only)")]
    public static string VKBeMcpShutdown()
    {
        // Delay 1 second before exiting to allow the response to be sent
        _ = Task.Delay(1000).ContinueWith(_ => Environment.Exit(0));

        return "Server is shutting down... You can start building in 1 second.";
    }
}

