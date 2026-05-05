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
    private static string? _cachedProjectRoot;

    private static string FindProjectRoot()
    {
        if (_cachedProjectRoot != null)
            return _cachedProjectRoot;

        var currentDir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(currentDir))
        {
            if (Directory.GetFiles(currentDir, "*.sln").Length > 0 || Directory.GetFiles(currentDir, "Directory.Build.props").Length > 0)
            {
                _cachedProjectRoot = currentDir;
                return currentDir;
            }
            currentDir = Path.GetDirectoryName(currentDir);
        }
        return AppContext.BaseDirectory;
    }
}
