using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Blocks.Generators.Authentication.Internal;
using VK.Blocks.Generators.Extensions;
using VK.Blocks.Generators.Utilities;

namespace VK.Blocks.Generators.Authentication;

/// <summary>
/// Source generator that automatically discovers and registers all concrete implementations of IVKClaimsProvider.
/// Always emits the <c>AddGeneratedClaimsProviders</c> extension method so that consuming assemblies
/// can call it unconditionally regardless of whether any providers are found.
/// </summary>
[Generator]
public sealed class ClaimsProviderDiscoveryGenerator : IIncrementalGenerator
{
    private const string TargetInterfaceName = "IVKClaimsProvider";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Capture the assembly name to filter out non-Authentication assemblies.
        var assemblyName = context.CompilationProvider.Select(
            static (c, _) => c.AssemblyName);

        var providers = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: IsLikelyProviderClass,
                transform: GetProviderInfo)
            .WhereNotNull();

        var combined = providers.Collect().Combine(assemblyName);
        context.RegisterSourceOutput(combined, (spc, pair) => Execute(spc, pair.Left, pair.Right, this.GetType()));
    }

    private static bool IsLikelyProviderClass(SyntaxNode node, System.Threading.CancellationToken _)
    {
        return node is ClassDeclarationSyntax { BaseList: not null };
    }

    private static ProviderInfo? GetProviderInfo(GeneratorSyntaxContext context, System.Threading.CancellationToken _)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        // Skip abstract or generic classes
        if (classDeclaration.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.AbstractKeyword)) ||
            classDeclaration.TypeParameterList is not null)
        {
            return null;
        }

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        // Only register classes defined in the current assembly to avoid redundant registrations
        if (!SymbolEqualityComparer.Default.Equals(classSymbol.ContainingAssembly, context.SemanticModel.Compilation.Assembly))
        {
            return null;
        }

        var implementsInterface = classSymbol.AllInterfaces.Any(i => i.Name == TargetInterfaceName);

        return implementsInterface ? new ProviderInfo(classSymbol.ToDisplayString()) : null;
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<ProviderInfo> providers, string? assemblyName, Type generatorType)
    {
        // Guard against execution in unrelated assemblies
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
        {
            return;
        }

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using VK.Blocks.Authentication.Abstractions;");
        sb.AppendLine();
        sb.AppendLine("namespace VK.Blocks.Authentication.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    internal static class GeneratedClaimsProviderExtensions");
        sb.AppendLine("    {");
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Automatically registers all IVKClaimsProvider implementations discovered in this assembly.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        public static IServiceCollection AddGeneratedClaimsProviders(this IServiceCollection services)");
        sb.AppendLine("        {");

        var providerList = providers.IsDefaultOrEmpty
            ? []
            : providers.OrderBy(p => p.FullName).ToList();

        foreach (var provider in providerList)
        {
            sb.AppendLine($"        services.AddScoped<IVKClaimsProvider, {provider.FullName}>();");
        }

        sb.AppendLine();
        sb.AppendLine("        return services;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource("GeneratedClaimsProviderExtensions.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(sb.ToString(), Encoding.UTF8));
    }


}
