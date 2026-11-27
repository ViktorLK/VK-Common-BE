using System.Collections.Generic;

namespace VK.Tools.SourceGenerators.AI.Internal;

internal enum SystemParameterKind
{
    None = 0,
    CancellationToken = 1,
    ExecutionContext = 2
}

internal sealed record ToolParameterInfo(
    string Name,
    string TypeName,
    bool IsNullable,
    bool IsRequired,
    string? Description,
    SystemParameterKind SystemKind = SystemParameterKind.None);

internal sealed record ToolTargetInfo(
    string ContainingNamespace,
    string TargetClassName,
    string TargetAccessibility,
    bool IsClassTool,
    string MethodName,
    string ReturnTypeName,
    bool ReturnsTask,
    bool ReturnsVKResult,
    bool ReturnsVoid,
    bool ReturnsAtomicToolResult,
    bool IsStatic,
    string ToolName,
    string Description,
    string? Category,
    IReadOnlyList<string> Tags,
    bool IsReadOnly,
    bool IsDestructive,
    bool IsIdempotent,
    bool IsOpenWorld,
    bool IsConcurrencySafe,
    int ApprovalMode,
    int? TimeoutMilliseconds,
    IReadOnlyList<ToolParameterInfo> Parameters);
