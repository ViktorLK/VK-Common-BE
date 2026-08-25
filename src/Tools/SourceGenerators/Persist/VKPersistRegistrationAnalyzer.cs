using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using VK.Tools.SourceGenerators.Diagnostics;

namespace VK.Tools.SourceGenerators.Persist;

/// <summary>
/// Roslyn Diagnostic Analyzer that ensures any project declaring [VKPersistEntity]
/// explicitly registers its generated persistence repositories and model contributors in DI.
/// Complies with AP.02 and BB.03.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class VKPersistRegistrationAnalyzer : DiagnosticAnalyzer
{
    private const string PersistEntityAttributeName = "VKPersistEntityAttribute";
    private const string PersistEntityAttributeFullName = "VK.Blocks.Core.VKPersistEntityAttribute";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [VKDiagnosticDescriptors.MissingPersistenceRepositoriesRegistration, VKDiagnosticDescriptors.MissingModelContributorsRegistration];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var compilation = context.Compilation;

        // Find all [VKPersistEntity] classes in this compilation
        var entityClasses = compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot(context.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>())
            .Where(cls =>
            {
                var semanticModel = compilation.GetSemanticModel(cls.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(cls, context.CancellationToken) is not INamedTypeSymbol symbol)
                    return false;

                return symbol.GetAttributes().Any(a =>
                    a.AttributeClass?.Name is PersistEntityAttributeName or "VKPersistEntity" ||
                    a.AttributeClass?.ToDisplayString() == PersistEntityAttributeFullName);
            })
            .ToList();

        if (entityClasses.Count == 0)
        {
            return; // No entities in this assembly, no registration required
        }

        // Check if the assembly is a BuildingBlock with [VKBlockMarker]
        // If a Block has [VKBlockMarker], we verify the marker or its partial method
        var allInvocationTexts = compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot(context.CancellationToken).DescendantNodes().OfType<InvocationExpressionSyntax>())
            .Select(inv => inv.Expression.ToString())
            .ToImmutableHashSet();

        bool hasRepositoriesCall = allInvocationTexts.Any(txt =>
            txt.EndsWith("AddGeneratedPersistenceRepositories", System.StringComparison.Ordinal) ||
            txt.Contains("AddGeneratedPersistenceRepositories("));

        bool hasModelContributorsCall = allInvocationTexts.Any(txt =>
            txt.EndsWith("AddGeneratedModelContributors", System.StringComparison.Ordinal) ||
            txt.Contains("AddGeneratedModelContributors("));

        var targetLocation = entityClasses.First().Identifier.GetLocation();
        var assemblyName = compilation.AssemblyName ?? "Assembly";

        if (!hasRepositoriesCall)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                VKDiagnosticDescriptors.MissingPersistenceRepositoriesRegistration,
                targetLocation,
                assemblyName));
        }

        if (!hasModelContributorsCall)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                VKDiagnosticDescriptors.MissingModelContributorsRegistration,
                targetLocation,
                assemblyName));
        }
    }
}
