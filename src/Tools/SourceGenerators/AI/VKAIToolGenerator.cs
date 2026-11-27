using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.AI.Internal;
using VK.Tools.SourceGenerators.Extensions;
using VK.Tools.SourceGenerators.Utilities;

namespace VK.Tools.SourceGenerators.AI;

/// <summary>
/// Source generator that generates compile-time IVKAtomicTool implementations from [VKAITool] classes or methods.
/// Guarantees zero runtime reflection (AP.01).
/// </summary>
[Generator]
public sealed class VKAIToolGenerator : IIncrementalGenerator
{
    private const string ToolAttributeName = "VKAITool";
    private const string ToolAttributeFullName = "VK.Blocks.AI.VKAIToolAttribute";
    private const string ParamAttributeFullName = "VK.Blocks.AI.VKAIToolParameterAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var toolTargets = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidate(node),
                transform: static (ctx, ct) => Transform(ctx, ct))
            .WhereNotNull();

        var combined = toolTargets.Collect().Combine(assemblyName);
        context.RegisterSourceOutput(combined, (spc, pair) => Execute(spc, pair.Left, pair.Right, this.GetType()));
    }

    private static bool IsCandidate(SyntaxNode node)
    {
        return (node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0) ||
               (node is MethodDeclarationSyntax m && m.AttributeLists.Count > 0);
    }

    private static ToolTargetInfo? Transform(GeneratorSyntaxContext context, System.Threading.CancellationToken ct)
    {
        if (context.Node is ClassDeclarationSyntax classSyntax)
        {
            if (context.SemanticModel.GetDeclaredSymbol(classSyntax, ct) is not INamedTypeSymbol classSymbol)
                return null;

            var toolAttr = FindAttribute(classSymbol, ToolAttributeFullName, ToolAttributeName);
            if (toolAttr is null)
                return null;

            // Find an execution method (ExecuteAsync or Execute)
            var executeMethod = classSymbol.GetMembers().OfType<IMethodSymbol>()
                .FirstOrDefault(m => m.Name is "ExecuteAsync" or "Execute" && m.DeclaredAccessibility == Accessibility.Public);

            if (executeMethod is null)
                return null;

            return BuildTargetInfo(classSymbol, executeMethod, toolAttr, isClassTool: true);
        }
        else if (context.Node is MethodDeclarationSyntax methodSyntax)
        {
            if (context.SemanticModel.GetDeclaredSymbol(methodSyntax, ct) is not IMethodSymbol methodSymbol)
                return null;

            var toolAttr = FindAttribute(methodSymbol, ToolAttributeFullName, ToolAttributeName);
            if (toolAttr is null)
                return null;

            var containingClass = methodSymbol.ContainingType;
            if (containingClass is null)
                return null;

            return BuildTargetInfo(containingClass, methodSymbol, toolAttr, isClassTool: false);
        }

        return null;
    }

    private static AttributeData? FindAttribute(ISymbol symbol, string fullName, string shortName)
    {
        return symbol.GetAttributes().FirstOrDefault(a =>
        {
            var name = a.AttributeClass?.ToDisplayString();
            return name == fullName || name == shortName || a.AttributeClass?.Name == shortName || a.AttributeClass?.Name == shortName + "Attribute";
        });
    }

    private static ToolTargetInfo BuildTargetInfo(INamedTypeSymbol classSymbol, IMethodSymbol methodSymbol, AttributeData toolAttr, bool isClassTool)
    {
        string? customName = null;
        string? description = null;
        string? category = null;
        var tags = new List<string>();
        var isReadOnly = false;
        var isDestructive = false;
        var isIdempotent = false;
        var isOpenWorld = false;
        var isConcurrencySafe = true;
        var approvalMode = 0;
        int? timeoutMs = null;

        if (toolAttr.ConstructorArguments.Length > 0)
        {
            customName = toolAttr.ConstructorArguments[0].Value as string;
        }
        if (toolAttr.ConstructorArguments.Length > 1)
        {
            description = toolAttr.ConstructorArguments[1].Value as string;
        }

        foreach (var named in toolAttr.NamedArguments)
        {
            switch (named.Key)
            {
                case "Name":
                    customName = named.Value.Value as string;
                    break;
                case "Description":
                    description = named.Value.Value as string;
                    break;
                case "Category":
                    category = named.Value.Value as string;
                    break;
                case "IsReadOnly":
                    isReadOnly = named.Value.Value is true;
                    break;
                case "IsDestructive":
                    isDestructive = named.Value.Value is true;
                    break;
                case "IsIdempotent":
                    isIdempotent = named.Value.Value is true;
                    break;
                case "IsOpenWorld":
                    isOpenWorld = named.Value.Value is true;
                    break;
                case "IsConcurrencySafe":
                    isConcurrencySafe = named.Value.Value is not false;
                    break;
                case "ApprovalMode":
                    if (named.Value.Value is not null) approvalMode = Convert.ToInt32(named.Value.Value);
                    break;
                case "RequiresConfirmation":
                    if (named.Value.Value is true && approvalMode == 0) approvalMode = 1;
                    break;
                case "TimeoutMilliseconds":
                    if (named.Value.Value is int t && t > 0) timeoutMs = t;
                    break;
                case "Tags":
                    if (named.Value.Values is { Length: > 0 } arr)
                    {
                        foreach (var elem in arr)
                        {
                            if (elem.Value is string s && !string.IsNullOrWhiteSpace(s))
                                tags.Add(s);
                        }
                    }
                    break;
            }
        }

        var effectiveToolName = !string.IsNullOrWhiteSpace(customName)
            ? customName!
            : (isClassTool ? classSymbol.Name : $"{classSymbol.Name}_{methodSymbol.Name}");

        var effectiveDescription = !string.IsNullOrWhiteSpace(description)
            ? description!
            : $"Executes {effectiveToolName}.";

        var targetAccessibility = classSymbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            _ => "internal"
        };

        var returnType = methodSymbol.ReturnType;
        var returnTypeStr = returnType.ToDisplayString();
        var returnsTask = returnType.Name == "Task" || returnTypeStr.StartsWith("System.Threading.Tasks.Task") ||
                          returnType.Name == "ValueTask" || returnTypeStr.StartsWith("System.Threading.Tasks.ValueTask");
        var returnsVoid = returnType.SpecialType == SpecialType.System_Void ||
                          (returnsTask && returnType is INamedTypeSymbol nts && !nts.IsGenericType);
        var returnsVKResult = returnTypeStr.Contains("VKResult");
        var returnsAtomicToolResult = returnTypeStr.Contains("VKAtomicToolResult");
        var isStatic = methodSymbol.IsStatic;

        var parameters = new List<ToolParameterInfo>();
        foreach (var param in methodSymbol.Parameters)
        {
            var paramTypeStr = param.Type.ToDisplayString();
            if (paramTypeStr == "System.Threading.CancellationToken" ||
                paramTypeStr.EndsWith(".CancellationToken"))
            {
                parameters.Add(new ToolParameterInfo(
                    Name: param.Name,
                    TypeName: param.Type.ToDisplayString(),
                    IsNullable: false,
                    IsRequired: false,
                    Description: null,
                    SystemKind: SystemParameterKind.CancellationToken));
                continue;
            }

            if (paramTypeStr == "VK.Blocks.AI.VKAtomicToolContext" ||
                paramTypeStr.EndsWith(".VKAtomicToolContext") ||
                paramTypeStr == "VKAtomicToolContext")
            {
                parameters.Add(new ToolParameterInfo(
                    Name: param.Name,
                    TypeName: param.Type.ToDisplayString(),
                    IsNullable: false,
                    IsRequired: false,
                    Description: null,
                    SystemKind: SystemParameterKind.ExecutionContext));
                continue;
            }

            var paramAttr = FindAttribute(param, ParamAttributeFullName, "VKAIToolParameter");
            string? paramDesc = null;
            var isRequired = param.NullableAnnotation != NullableAnnotation.Annotated && !param.HasExplicitDefaultValue;

            if (paramAttr != null)
            {
                if (paramAttr.ConstructorArguments.Length > 0)
                {
                    paramDesc = paramAttr.ConstructorArguments[0].Value as string;
                }
                foreach (var named in paramAttr.NamedArguments)
                {
                    if (named.Key == "Description")
                        paramDesc = named.Value.Value as string;
                    else if (named.Key == "Required" && named.Value.Value is bool req)
                        isRequired = req;
                }
            }

            parameters.Add(new ToolParameterInfo(
                Name: param.Name,
                TypeName: param.Type.ToDisplayString(),
                IsNullable: param.NullableAnnotation == NullableAnnotation.Annotated,
                IsRequired: isRequired,
                Description: paramDesc ?? $"The {param.Name} parameter.",
                SystemKind: SystemParameterKind.None));
        }

        return new ToolTargetInfo(
            ContainingNamespace: classSymbol.ContainingNamespace.ToDisplayString(),
            TargetClassName: classSymbol.Name,
            TargetAccessibility: targetAccessibility,
            IsClassTool: isClassTool,
            MethodName: methodSymbol.Name,
            ReturnTypeName: returnType.ToDisplayString(),
            ReturnsTask: returnsTask,
            ReturnsVKResult: returnsVKResult,
            ReturnsVoid: returnsVoid,
            ReturnsAtomicToolResult: returnsAtomicToolResult,
            IsStatic: isStatic,
            ToolName: effectiveToolName,
            Description: effectiveDescription,
            Category: category,
            Tags: tags,
            IsReadOnly: isReadOnly,
            IsDestructive: isDestructive,
            IsIdempotent: isIdempotent,
            IsOpenWorld: isOpenWorld,
            IsConcurrencySafe: isConcurrencySafe,
            ApprovalMode: approvalMode,
            TimeoutMilliseconds: timeoutMs,
            Parameters: parameters);
    }

    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<ToolTargetInfo> targets,
        string? assemblyName,
        Type generatorType)
    {
        if (targets.IsEmpty)
            return;

        foreach (var target in targets)
        {
            var code = GenerateToolClass(target, out var toolClassName);
            var hintName = $"{target.ContainingNamespace}.{toolClassName}.g.cs";
            context.AddSource(hintName, code);
        }
    }

    private static string GenerateToolClass(ToolTargetInfo target, out string toolClassName)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Text.Json;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using VK.Blocks.AI;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.ContainingNamespace};");
        sb.AppendLine();

        toolClassName = target.IsClassTool
            ? $"{target.TargetClassName}GeneratedAtomicTool"
            : $"{target.TargetClassName}_{target.MethodName}GeneratedAtomicTool";

        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Compile-time generated <see cref=\"IVKAtomicTool\"/> wrapper for {target.TargetClassName}.");
        sb.AppendLine("/// Guarantees zero runtime reflection (AP.01).");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"{target.TargetAccessibility} sealed class {toolClassName} : IVKAtomicTool");
        sb.AppendLine("{");

        // Field for service instance if non-static
        if (!target.IsStatic)
        {
            sb.AppendLine($"    private readonly {target.TargetClassName} _instance;");
            sb.AppendLine();
            sb.AppendLine($"    public {toolClassName}({target.TargetClassName} instance)");
            sb.AppendLine("    {");
            sb.AppendLine("        _instance = VKGuard.NotNull(instance);");
            sb.AppendLine("    }");
        }
        else
        {
            sb.AppendLine($"    public {toolClassName}()");
            sb.AppendLine("    {");
            sb.AppendLine("    }");
        }
        sb.AppendLine();

        // Manifest property
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine("    public VKAtomicToolManifest Manifest { get; } = new()");
        sb.AppendLine("    {");
        sb.AppendLine("        Metadata = new VKAtomicToolMetadata");
        sb.AppendLine("        {");
        sb.AppendLine($"            Name = \"{EscapeString(target.ToolName)}\",");
        sb.AppendLine($"            Description = \"{EscapeString(target.Description)}\",");
        if (!string.IsNullOrWhiteSpace(target.Category))
        {
            sb.AppendLine($"            Category = \"{EscapeString(target.Category!)}\",");
        }
        if (target.Tags.Count > 0)
        {
            var tagsCode = string.Join(", ", target.Tags.Select(t => $"\"{EscapeString(t)}\""));
            sb.AppendLine($"            Tags = [{tagsCode}],");
        }
        if (target.IsReadOnly)
        {
            sb.AppendLine("            IsReadOnly = true,");
        }
        if (target.IsDestructive)
        {
            sb.AppendLine("            IsDestructive = true,");
        }
        if (target.IsIdempotent)
        {
            sb.AppendLine("            IsIdempotent = true,");
        }
        if (target.IsOpenWorld)
        {
            sb.AppendLine("            IsOpenWorld = true,");
        }
        if (!target.IsConcurrencySafe)
        {
            sb.AppendLine("            IsConcurrencySafe = false,");
        }
        if (target.ApprovalMode != 0)
        {
            sb.AppendLine($"            ApprovalMode = (VKToolApprovalMode){target.ApprovalMode},");
        }
        if (target.TimeoutMilliseconds.HasValue)
        {
            sb.AppendLine($"            TimeoutMilliseconds = {target.TimeoutMilliseconds.Value},");
        }
        sb.AppendLine("        },");

        // Schema (only business parameters)
        var businessParams = target.Parameters.Where(p => p.SystemKind == SystemParameterKind.None).ToList();
        var paramSchema = BuildJsonSchema(businessParams);
        sb.AppendLine($"        ParameterSchema = \"\"\"\n{paramSchema}\n\"\"\",");
        sb.AppendLine("    };");
        sb.AppendLine();

        // ExecuteAsync
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine("    public async Task<VKResult<VKAtomicToolResult>> ExecuteAsync(");
        sb.AppendLine("        IDictionary<string, object> arguments,");
        sb.AppendLine("        VKAtomicToolContext context,");
        sb.AppendLine("        CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        VKGuard.NotNull(arguments);");
        sb.AppendLine("        VKGuard.NotNull(context);");
        sb.AppendLine();

        // Parameter unpacking and type-casting (only business parameters)
        foreach (var p in businessParams)
        {
            sb.AppendLine($"        if (!arguments.TryGetValue(\"{p.Name}\", out var {p.Name}_raw) || {p.Name}_raw is null)");
            sb.AppendLine("        {");
            if (p.IsRequired)
            {
                sb.AppendLine($"            return VKResult.Failure<VKAtomicToolResult>(VKAIErrors.InvalidRequest(\"Missing required tool argument: '{p.Name}'.\"));");
            }
            else
            {
                sb.AppendLine($"            {p.Name}_raw = default({p.TypeName});");
            }
            sb.AppendLine("        }");
            var nullForgiving = p.IsNullable ? "" : "!";
            sb.AppendLine($"        var {p.Name}_val = ConvertArg<{p.TypeName}>({p.Name}_raw){nullForgiving};");
        }
        sb.AppendLine();

        // Invocation
        sb.AppendLine("        try");
        sb.AppendLine("        {");

        var invocationArgs = new List<string>();
        foreach (var p in target.Parameters)
        {
            if (p.SystemKind == SystemParameterKind.CancellationToken)
            {
                invocationArgs.Add("cancellationToken");
            }
            else if (p.SystemKind == SystemParameterKind.ExecutionContext)
            {
                invocationArgs.Add("context");
            }
            else
            {
                invocationArgs.Add($"{p.Name}_val");
            }
        }

        var targetPrefix = target.IsStatic ? target.TargetClassName : "_instance";
        var callStr = $"{targetPrefix}.{target.MethodName}({string.Join(", ", invocationArgs)})";

        if (target.ReturnsVoid)
        {
            if (target.ReturnsTask)
            {
                sb.AppendLine($"            await {callStr}.ConfigureAwait(false);");
            }
            else
            {
                sb.AppendLine($"            {callStr};");
            }
            sb.AppendLine("            return VKResult.Success(new VKAtomicToolResult { Content = \"Operation completed successfully.\" });");
        }
        else if (target.ReturnsAtomicToolResult)
        {
            if (target.ReturnsTask)
            {
                if (target.ReturnsVKResult)
                {
                    sb.AppendLine($"            return await {callStr}.ConfigureAwait(false);");
                }
                else
                {
                    sb.AppendLine($"            var res = await {callStr}.ConfigureAwait(false);");
                    sb.AppendLine("            return VKResult.Success(res);");
                }
            }
            else
            {
                if (target.ReturnsVKResult)
                {
                    sb.AppendLine($"            return {callStr};");
                }
                else
                {
                    sb.AppendLine($"            var res = {callStr};");
                    sb.AppendLine("            return VKResult.Success(res);");
                }
            }
        }
        else
        {
            if (target.ReturnsTask)
            {
                if (target.ReturnsVKResult)
                {
                    sb.AppendLine($"            var res = await {callStr}.ConfigureAwait(false);");
                    sb.AppendLine("            if (!res.IsSuccess)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                return VKResult.Failure<VKAtomicToolResult>(res.FirstError);");
                    sb.AppendLine("            }");
                    sb.AppendLine("            var content = res.Value?.ToString() ?? string.Empty;");
                    sb.AppendLine("            return VKResult.Success(new VKAtomicToolResult { Content = content });");
                }
                else
                {
                    sb.AppendLine($"            var res = await {callStr}.ConfigureAwait(false);");
                    sb.AppendLine("            var content = res?.ToString() ?? string.Empty;");
                    sb.AppendLine("            return VKResult.Success(new VKAtomicToolResult { Content = content });");
                }
            }
            else
            {
                if (target.ReturnsVKResult)
                {
                    sb.AppendLine($"            var res = {callStr};");
                    sb.AppendLine("            if (!res.IsSuccess)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                return VKResult.Failure<VKAtomicToolResult>(res.FirstError);");
                    sb.AppendLine("            }");
                    sb.AppendLine("            var content = res.Value?.ToString() ?? string.Empty;");
                    sb.AppendLine("            return VKResult.Success(new VKAtomicToolResult { Content = content });");
                }
                else
                {
                    sb.AppendLine($"            var res = {callStr};");
                    sb.AppendLine("            var content = res?.ToString() ?? string.Empty;");
                    sb.AppendLine("            return VKResult.Success(new VKAtomicToolResult { Content = content });");
                }
            }
        }

        sb.AppendLine("        }");
        sb.AppendLine("        catch (OperationCanceledException)");
        sb.AppendLine("        {");
        sb.AppendLine("            return VKResult.Failure<VKAtomicToolResult>(VKAIErrors.Timeout);");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine("            return VKResult.Failure<VKAtomicToolResult>(VKAIErrors.InvalidRequest(ex.Message));");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    private static readonly JsonSerializerOptions JsonWebOptions = new(JsonSerializerDefaults.Web);");
        sb.AppendLine();
        // Helper conversion method
        sb.AppendLine("    private static T? ConvertArg<T>(object? val)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (val is null) return default;");
        sb.AppendLine("        if (val is T exact) return exact;");
        sb.AppendLine();
        sb.AppendLine("        if (val is JsonElement elem)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (elem.ValueKind == JsonValueKind.Null || elem.ValueKind == JsonValueKind.Undefined)");
        sb.AppendLine("            {");
        sb.AppendLine("                return default;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            var targetType = typeof(T);");
        sb.AppendLine("            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;");
        sb.AppendLine();
        sb.AppendLine("            if (underlyingType == typeof(string))");
        sb.AppendLine("            {");
        sb.AppendLine("                return (T)(object)(elem.GetString() ?? elem.GetRawText());");
        sb.AppendLine("            }");
        sb.AppendLine("            if (underlyingType == typeof(bool))");
        sb.AppendLine("            {");
        sb.AppendLine("                if (elem.ValueKind == JsonValueKind.True || elem.ValueKind == JsonValueKind.False) return (T)(object)elem.GetBoolean();");
        sb.AppendLine("                if (bool.TryParse(elem.GetString(), out var b)) return (T)(object)b;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (underlyingType == typeof(int))");
        sb.AppendLine("            {");
        sb.AppendLine("                if (elem.TryGetInt32(out var i)) return (T)(object)i;");
        sb.AppendLine("                if (int.TryParse(elem.GetString(), out var si)) return (T)(object)si;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (underlyingType == typeof(long))");
        sb.AppendLine("            {");
        sb.AppendLine("                if (elem.TryGetInt64(out var l)) return (T)(object)l;");
        sb.AppendLine("                if (long.TryParse(elem.GetString(), out var sl)) return (T)(object)sl;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (underlyingType == typeof(double))");
        sb.AppendLine("            {");
        sb.AppendLine("                if (elem.TryGetDouble(out var d)) return (T)(object)d;");
        sb.AppendLine("                if (double.TryParse(elem.GetString(), out var sd)) return (T)(object)sd;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (underlyingType == typeof(float))");
        sb.AppendLine("            {");
        sb.AppendLine("                if (elem.TryGetSingle(out var f)) return (T)(object)f;");
        sb.AppendLine("                if (float.TryParse(elem.GetString(), out var sf)) return (T)(object)sf;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (underlyingType == typeof(decimal))");
        sb.AppendLine("            {");
        sb.AppendLine("                if (elem.TryGetDecimal(out var dec)) return (T)(object)dec;");
        sb.AppendLine("                if (decimal.TryParse(elem.GetString(), out var sdec)) return (T)(object)sdec;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (underlyingType == typeof(Guid))");
        sb.AppendLine("            {");
        sb.AppendLine("                if (elem.TryGetGuid(out var g)) return (T)(object)g;");
        sb.AppendLine("                if (Guid.TryParse(elem.GetString(), out var sg)) return (T)(object)sg;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (underlyingType == typeof(DateTimeOffset))");
        sb.AppendLine("            {");
        sb.AppendLine("                if (elem.TryGetDateTimeOffset(out var dto)) return (T)(object)dto;");
        sb.AppendLine("                if (DateTimeOffset.TryParse(elem.GetString(), out var sdto)) return (T)(object)sdto;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (underlyingType == typeof(DateTime))");
        sb.AppendLine("            {");
        sb.AppendLine("                if (elem.TryGetDateTime(out var dt)) return (T)(object)dt;");
        sb.AppendLine("                if (DateTime.TryParse(elem.GetString(), out var sdt)) return (T)(object)sdt;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (underlyingType.IsEnum)");
        sb.AppendLine("            {");
        sb.AppendLine("                var str = elem.GetString();");
        sb.AppendLine("                if (str is not null && Enum.TryParse(underlyingType, str, ignoreCase: true, out var enumVal))");
        sb.AppendLine("                {");
        sb.AppendLine("                    return (T)enumVal;");
        sb.AppendLine("                }");
        sb.AppendLine("                if (elem.TryGetInt32(out var intVal))");
        sb.AppendLine("                {");
        sb.AppendLine("                    return (T)Enum.ToObject(underlyingType, intVal);");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            try");
        sb.AppendLine("            {");
        sb.AppendLine("                return JsonSerializer.Deserialize<T>(elem.GetRawText(), JsonWebOptions);");
        sb.AppendLine("            }");
        sb.AppendLine("            catch");
        sb.AppendLine("            {");
        sb.AppendLine("                return default;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine("            var targetType = typeof(T);");
        sb.AppendLine("            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;");
        sb.AppendLine("            if (underlyingType.IsEnum && val is string strVal)");
        sb.AppendLine("            {");
        sb.AppendLine("                return (T)Enum.Parse(underlyingType, strVal, ignoreCase: true);");
        sb.AppendLine("            }");
        sb.AppendLine("            return (T)Convert.ChangeType(val, underlyingType);");
        sb.AppendLine("        }");
        sb.AppendLine("        catch");
        sb.AppendLine("        {");
        sb.AppendLine("            try");
        sb.AppendLine("            {");
        sb.AppendLine("                var json = JsonSerializer.Serialize(val, JsonWebOptions);");
        sb.AppendLine("                return JsonSerializer.Deserialize<T>(json, JsonWebOptions);");
        sb.AppendLine("            }");
        sb.AppendLine("            catch");
        sb.AppendLine("            {");
        sb.AppendLine("                return default;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("    }");

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildJsonSchema(IReadOnlyList<ToolParameterInfo> parameters)
    {
        var sb = new StringBuilder();
        sb.AppendLine("        {");
        sb.AppendLine("            \"type\": \"object\",");
        sb.AppendLine("            \"properties\": {");

        for (int i = 0; i < parameters.Count; i++)
        {
            var p = parameters[i];
            var jsonType = MapToJsonSchemaType(p.TypeName);
            var isLast = i == parameters.Count - 1;
            sb.AppendLine($"                \"{p.Name}\": {{");
            sb.AppendLine($"                    \"type\": \"{jsonType}\",");
            sb.AppendLine($"                    \"description\": \"{EscapeString(p.Description ?? p.Name)}\"");
            sb.Append("                }");
            if (!isLast) sb.Append(",");
            sb.AppendLine();
        }

        sb.AppendLine("            },");

        var required = parameters.Where(p => p.IsRequired).Select(p => $"\"{p.Name}\"").ToList();
        if (required.Count > 0)
        {
            sb.AppendLine($"            \"required\": [{string.Join(", ", required)}],");
        }
        else
        {
            sb.AppendLine("            \"required\": [],");
        }

        sb.AppendLine("            \"additionalProperties\": false");
        sb.Append("        }");
        return sb.ToString();
    }

    private static string MapToJsonSchemaType(string typeName)
    {
        if (typeName.EndsWith("[]") ||
            typeName.StartsWith("System.Collections.Generic.IEnumerable") ||
            typeName.StartsWith("System.Collections.Generic.IList") ||
            typeName.StartsWith("System.Collections.Generic.List") ||
            typeName.StartsWith("System.Collections.Generic.IReadOnlyList"))
        {
            return "array";
        }

        return typeName switch
        {
            "string" or "System.String" => "string",
            "int" or "System.Int32" or "long" or "System.Int64" or "short" or "System.Int16" => "integer",
            "float" or "System.Single" or "double" or "System.Double" or "decimal" or "System.Decimal" => "number",
            "bool" or "System.Boolean" => "boolean",
            "object" or "System.Object" => "object",
            _ => "string"
        };
    }

    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "")
            .Replace("\n", "\\n");
    }
}

