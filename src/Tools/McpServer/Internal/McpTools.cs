using System;
using System.IO;
using ModelContextProtocol.Server;

namespace VK.Tools.McpServer.Internal;

/// <summary>
/// Implementation of the VK.Blocks management tools.
/// This class is partial and its tools are implemented in separate files.
/// </summary>
[McpServerToolType]
internal sealed partial class McpTools
{
    private static string FindProjectRoot()
    {
        var currentDir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(currentDir))
        {
            if (Directory.Exists(Path.Combine(currentDir, ".agents")) ||
                Directory.GetFiles(currentDir, "*.sln").Length > 0 ||
                Directory.GetFiles(currentDir, "Directory.Build.props").Length > 0)
            {
                return currentDir;
            }
            var parent = Path.GetDirectoryName(currentDir);
            if (parent == currentDir) break;
            currentDir = parent;
        }
        return AppContext.BaseDirectory;
    }
}
