using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Blocks.Generators.Authorization.Internal;
using VK.Blocks.Generators.Extensions;
using VK.Blocks.Generators.Utilities;

namespace VK.Blocks.Generators.Authorization;

/// <summary>
/// Scans for enums decorated with [GenerateRankAuthorize] and generates
/// corresponding Authorization Policies and AuthorizeAttributes.
/// </summary>
[Generator]
public sealed class MinimumRankGenerator : IIncrementalGenerator
{
    private const string AttributeName = "GenerateRankAuthorize";
    private const string AttributeFullName = VKBlocksConstants.VKBlocksPrefix + "Authorization.Features.MinimumRank.Metadata." + AttributeName;

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var enumInfos = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is EnumDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: GetRankEnumInfo)
            .WhereNotNull();

        var combined = enumInfos.Collect().Combine(assemblyName);
        context.RegisterSourceOutput(combined, (spc, pair) => Execute(spc, pair.Left, pair.Right, this.GetType()));
    }

    private static RankEnumInfo? GetRankEnumInfo(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var enumDeclaration = (EnumDeclarationSyntax)context.Node;

        foreach (var attributeList in enumDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attribute, ct).Symbol is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }

                var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                var fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName == AttributeFullName || fullName == AttributeFullName + "Attribute")
                {
                    if (context.SemanticModel.GetDeclaredSymbol(enumDeclaration) is not INamedTypeSymbol enumSymbol)
                    {
                        return null;
                    }

                    var members = enumSymbol.GetMembers()
                        .OfType<IFieldSymbol>()
                        .Select(m => m.Name)
                        .ToList();

                    return new RankEnumInfo(
                        enumSymbol.ContainingNamespace.ToDisplayString(),
                        enumSymbol.Name,
                        members);
                }
            }
        }

        return null;
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<RankEnumInfo> enums, string? assemblyName, Type generatorType)
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

    private static void GenerateCode(SourceProductionContext context, RankEnumInfo info)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using Microsoft.AspNetCore.Authorization;");
        sb.AppendLine();
        sb.AppendLine("namespace VK.Blocks.Authorization.Generated");
        sb.AppendLine("{");

        var attributeName = $"Require{info.Name}Attribute";

        // Generate dynamic authorize attribute based on the enum name
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Marks a controller or action as requiring at least a specific <see cref=\"{info.Namespace}.{info.Name}\"/>.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = false)]");
        sb.AppendLine($"    public sealed class {attributeName} : AuthorizeAttribute, IAuthorizationRequirementData");
        sb.AppendLine("    {");
        sb.AppendLine($"        public {info.Namespace}.{info.Name} Rank {{ get; }}");
        sb.AppendLine();
        sb.AppendLine($"        public {attributeName}({info.Namespace}.{info.Name} rank)");
        sb.AppendLine("        {");
        sb.AppendLine("            Rank = rank;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        /// <inheritdoc />");
        sb.AppendLine("        public System.Collections.Generic.IEnumerable<IAuthorizationRequirement> GetRequirements()");
        sb.AppendLine("        {");
        sb.AppendLine($"            yield return new VK.Blocks.Authorization.Features.MinimumRank.MinimumRankRequirement((int)Rank, typeof({info.Namespace}.{info.Name}));");
        sb.AppendLine("        }");
        sb.AppendLine("    }");

        sb.AppendLine("}");

        context.AddSource($"{info.Name}.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
