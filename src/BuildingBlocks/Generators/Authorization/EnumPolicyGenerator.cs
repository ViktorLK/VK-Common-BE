using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Blocks.Generators.Authorization.Internal;
using VK.Blocks.Generators.Extensions;
using VK.Blocks.Generators.Utilities;

namespace VK.Blocks.Generators.Authorization;

/// <summary>
/// Generates Requirements, Attributes, and Handlers for enums decorated with [GeneratePolicy].
/// </summary>
[Generator]
public sealed class EnumPolicyGenerator : IIncrementalGenerator
{
    private const string AttributeName = "GeneratePolicy";
    private const string AttributeFullName = VKBlocksConstants.VKBlocksPrefix + "Authorization.Features.DynamicPolicies.Metadata." + AttributeName;

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var enumInfos = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is EnumDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: GetEnumPolicyInfo)
            .WhereNotNull();

        var combined = enumInfos.Collect().Combine(assemblyName);
        context.RegisterSourceOutput(combined, (spc, pair) => Execute(spc, pair.Left, pair.Right, this.GetType()));
    }

    private static EnumPolicyInfo? GetEnumPolicyInfo(GeneratorSyntaxContext context, System.Threading.CancellationToken ct)
    {
        var enumDeclaration = (EnumDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(enumDeclaration, ct) is not INamedTypeSymbol enumSymbol)
        {
            return null;
        }

        var attribute = enumSymbol.GetAttributes()
            .FirstOrDefault(a =>
            {
                var name = a.AttributeClass?.ToDisplayString();
                return name == AttributeFullName || name == AttributeFullName + "Attribute";
            });

        if (attribute is null)
        {
            return null;
        }

        var op = "GreaterThanOrEqual";
        string? claimType = null;

        foreach (var namedArg in attribute.NamedArguments)
        {
            if (namedArg.Key == "Operator")
            {
                // Operator is an enum: AuthorizationOperator
                // 0: Equals, 1: GreaterThanOrEqual, 2: LessThanOrEqual, 3: In
                op = namedArg.Value.Value switch
                {
                    0 => "Equals",
                    1 => "GreaterThanOrEqual",
                    2 => "LessThanOrEqual",
                    3 => "In",
                    _ => "GreaterThanOrEqual"
                };
            }
            else if (namedArg.Key == "ClaimType")
            {
                claimType = namedArg.Value.Value?.ToString();
            }
        }

        return new EnumPolicyInfo(
            enumSymbol.ContainingNamespace.ToDisplayString(),
            enumSymbol.Name,
            enumSymbol.ToDisplayString(),
            op,
            claimType);
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<EnumPolicyInfo> enums, string? assemblyName, Type generatorType)
    {
        // Guard against execution in unrelated assemblies
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
        {
            return;
        }

        if (enums.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var info in enums.Distinct())
        {
            GenerateCode(context, info);
        }
    }

    private static void GenerateCode(SourceProductionContext context, EnumPolicyInfo info)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Collections.Immutable;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Security.Claims;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.AspNetCore.Authorization;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine("using VK.Blocks.Authorization.Abstractions;");
        sb.AppendLine("using VK.Blocks.Authorization.Common;");
        sb.AppendLine("using VK.Blocks.Authorization.DependencyInjection;");
        sb.AppendLine("using VK.Blocks.Core.Results;");
        sb.AppendLine();
        sb.AppendLine("namespace VK.Blocks.Authorization.Generated");
        sb.AppendLine("{");

        var baseName = info.Name;
        var requirementName = $"{baseName}Requirement";
        var attributeName = $"Require{baseName}Attribute";
        var handlerName = $"{baseName}AuthorizationHandler";
        var isInOp = info.Operator == "In";

        // 1. Requirement
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Requirement for <see cref=\"{info.FullName}\"/> based authorization.");
        sb.AppendLine("    /// </summary>");
        if (isInOp)
        {
            sb.AppendLine($"    public sealed record {requirementName}(System.Collections.Immutable.ImmutableArray<{info.FullName}> RequiredValues) : IVKAuthorizationRequirement");
        }
        else
        {
            sb.AppendLine($"    public sealed record {requirementName}({info.FullName} RequiredValue) : IVKAuthorizationRequirement");
        }
        sb.AppendLine("    {");
        sb.AppendLine($"        public Error DefaultError => new Error(\"Authorization.{baseName}Mismatch\", \"Insufficient {baseName} level.\", ErrorType.Forbidden);");
        sb.AppendLine("    }");
        sb.AppendLine();

        // 2. Attribute
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Specifies that the decorated controller or action requires a specific <see cref=\"{info.FullName}\"/>.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    [System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]");
        sb.AppendLine($"    public sealed class {attributeName} : AuthorizeAttribute, IAuthorizationRequirementData");
        sb.AppendLine("    {");
        if (isInOp)
        {
            sb.AppendLine($"        public System.Collections.Immutable.ImmutableArray<{info.FullName}> Values {{ get; }}");
            sb.AppendLine($"        public {attributeName}(params {info.FullName}[] values) => Values = values.ToImmutableArray();");
        }
        else
        {
            sb.AppendLine($"        public {info.FullName} Value {{ get; }}");
            sb.AppendLine($"        public {attributeName}({info.FullName} value) => Value = value;");
        }
        sb.AppendLine();
        sb.AppendLine("        public IEnumerable<IAuthorizationRequirement> GetRequirements()");
        sb.AppendLine("        {");
        if (isInOp)
        {
            sb.AppendLine($"            yield return new {requirementName}(Values);");
        }
        else
        {
            sb.AppendLine($"            yield return new {requirementName}(Value);");
        }
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        // 3. Handler
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Authorization handler for <see cref=\"{requirementName}\"/>.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public sealed class {handlerName}(");
        sb.AppendLine("        IOptions<VKAuthorizationOptions> options,");
        sb.AppendLine($"        ILogger<{handlerName}> logger)");
        sb.AppendLine($"        : AuthorizationHandler<{requirementName}>");
        sb.AppendLine("    {");
        sb.AppendLine($"        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, {requirementName} requirement)");
        sb.AppendLine("        {");

        var claimType = info.ClaimType ?? $"vk.{info.Name.ToLowerInvariant()}";
        sb.AppendLine($"            var claimType = \"{claimType}\";");
        sb.AppendLine("            var claimValue = context.User.FindFirstValue(claimType);");
        sb.AppendLine();
        sb.AppendLine($"            if (Enum.TryParse<{info.FullName}>(claimValue, out var userValue))");
        sb.AppendLine("            {");

        // Logic based on operator
        var compareLogic = info.Operator switch
        {
            "Equals" => "userValue == requirement.RequiredValue",
            "GreaterThanOrEqual" => "userValue >= requirement.RequiredValue",
            "LessThanOrEqual" => "userValue <= requirement.RequiredValue",
            "In" => "requirement.RequiredValues.Contains(userValue)",
            _ => "userValue == requirement.RequiredValue"
        };

        sb.AppendLine($"                if ({compareLogic})");
        sb.AppendLine("                {");
        sb.AppendLine("                    context.Succeed(requirement);");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            return Task.CompletedTask;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");

        sb.AppendLine("}");

        context.AddSource($"{info.Name}Policy.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
