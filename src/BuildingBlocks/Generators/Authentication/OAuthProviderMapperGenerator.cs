using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using VK.Blocks.Generators.Authentication.Internal;
using VK.Blocks.Generators.Extensions;
using VK.Blocks.Generators.Utilities;

namespace VK.Blocks.Generators.Authentication;

/// <summary>
/// A Source Generator that discovers and registers OAuth claims mappers directly in DI.
/// </summary>
[Generator]
public sealed class OAuthProviderMapperGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = VKBlocksConstants.VKBlocksPrefix + "Authentication.Features.OAuth.Mappers.OAuthProviderAttribute";

    private static readonly DiagnosticDescriptor _duplicateProviderWarning = new(
        id: "VK0001",
        title: "Duplicate OAuth Provider Mapper",
        messageFormat: "Multiple OAuth mappers found for provider '{0}'. Only '{1}' will be registered.",
        category: VKBlocksConstants.VKBlocksPrefix + "Authentication",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Gather all classes decorated with [OAuthProvider]
        var mapperTargets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeFullName,
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: GetMapperInfo)
            .Where(m => m != null);

        // 2. Identify the current assembly name to ensure we only generate for the right project
        var assemblyName = context.CompilationProvider.Select((c, _) => c.AssemblyName);

        // 3. Generation logic - combined with assembly name
        context.RegisterSourceOutput(mapperTargets.Collect().Combine(assemblyName),
            (ctx, input) => EmitMapperExtensions(ctx, input.Left, input.Right, this.GetType()));
    }

    private static MapperInfo? GetMapperInfo(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var classSymbol = (INamedTypeSymbol)context.TargetSymbol;

        // Ensure the mapper is defined IN the current assembly to prevent capturing
        // mappers from referenced assemblies (e.g. OIDC project capturing Core mappers)
        if (!SymbolEqualityComparer.Default.Equals(classSymbol.ContainingAssembly, context.SemanticModel.Compilation.Assembly))
        {
            return null;
        }

        var attribute = context.Attributes[0];
        if (attribute.ConstructorArguments.Length == 0)
        {
            // SUGGEST: Use expression body (=>)
            return null;
        }

        var providerName = attribute.ConstructorArguments[0].Value?.ToString();
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return null;
        }

        return new MapperInfo(providerName!, classSymbol.ToDisplayString(), context.TargetNode.GetLocation());
    }

    private static void EmitMapperExtensions(SourceProductionContext context, ImmutableArray<MapperInfo?> mappers, string? assemblyName, Type generatorType)
    {
        // Guard against execution in unrelated assemblies (Rule 11)
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
        {
            return;
        }

        // Filter out nulls once
        var validMappers = mappers.Where(m => m != null).Cast<MapperInfo>().ToList();

        // Standardize the suffix for the filename only
        // SAFE: assemblyName is guaranteed to be non-null by the ShouldExecute guard above.
        var fileSuffix = assemblyName!.Replace(".", "");
        var prefix = assemblyName.EndsWith(".OpenIdConnect", StringComparison.Ordinal) ? "VKOidc" : "VKOAuth";

        var sb = SourceCodeBuilder.CreateWithHeader();

        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using VK.Blocks.Authentication.Features.OAuth;");
        sb.AppendLine("using VK.Blocks.Authentication.Features.OAuth.Mappers;");
        sb.AppendLine();
        sb.AppendLine("namespace VK.Blocks.Authentication.Generated");
        sb.AppendLine("{");
        sb.AppendLine($"    internal static class {prefix}OAuthMapperExtensions");
        sb.AppendLine("    {");
        sb.AppendLine($"        public static IServiceCollection Add{prefix}GeneratedMappers(this IServiceCollection services)");
        sb.AppendLine("        {");

        if (validMappers.Count == 0)
        {
            sb.AppendLine("            return services;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            context.AddSource($"{prefix}OAuthMapperExtensions_{fileSuffix}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));

            // Still generate empty metadata to avoid CS0103
            EmitMetadata(context, prefix, fileSuffix, [], assemblyName);
            return;
        }

        var grouped = validMappers.GroupBy(m => m.ProviderName);
        var providerNames = new List<string>();

        foreach (var group in grouped)
        {
            var mappersInGroup = group.ToList();
            var primary = mappersInGroup[0];
            providerNames.Add(primary.ProviderName);

            // Generate registration for the primary mapper
            sb.AppendLine($"        services.AddKeyedScoped(typeof(IOAuthClaimsMapper), \"{primary.ProviderName}\", typeof({primary.FullClassName}));");

            // Report warnings for any duplicates found for the same provider
            if (mappersInGroup.Count > 1)
            {
                foreach (var dup in mappersInGroup.Skip(1))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        _duplicateProviderWarning,
                        dup.Location,
                        primary.ProviderName,
                        primary.FullClassName));
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("            return services;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{prefix}OAuthMapperExtensions_{fileSuffix}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));

        // Generate the metadata class used for dynamic policy discovery
        EmitMetadata(context, prefix, fileSuffix, providerNames, assemblyName);
    }

    private static void EmitMetadata(SourceProductionContext context, string prefix, string fileSuffix, List<string> providerNames, string assemblyName)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine("namespace VK.Blocks.Authentication.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Compile-time discovered OAuth providers for the {assemblyName} assembly.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    internal static class {prefix}GeneratedMetadata");
        sb.AppendLine("    {");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// The list of all provider names that have a registered IOAuthClaimsMapper in this assembly.");
        sb.AppendLine("    /// </summary>");

        if (providerNames.Count == 0)
        {
            sb.AppendLine("    public static readonly IEnumerable<string> AllProviders = [];");
        }
        else
        {
            sb.AppendLine("    public static readonly IEnumerable<string> AllProviders =");
            sb.AppendLine("    [");
            for (var i = 0; i < providerNames.Count; i++)
            {
                var comma = i < providerNames.Count - 1 ? "," : "";
                sb.AppendLine($"        \"{providerNames[i]}\"{comma}");
            }
            sb.AppendLine("    ];");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{prefix}GeneratedMetadata_{fileSuffix}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
