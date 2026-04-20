using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using VK.Blocks.Generators.Diagnostics;
using VK.Blocks.Generators.Extensions;

namespace VK.Blocks.Generators.Observability;

/// <summary>
/// Analyzer that ensures all authorization handlers record their evaluation metrics to comply with Rule 6.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ObservabilityAnalyzer : DiagnosticAnalyzer
{
    private const string AuthorizationHandlerInterface = "IAuthorizationHandler";
    private const string DiagnosableAttribute = "DiagnosableAttribute";
    private const string RecordProcessMethodName = "RecordProcess";
    private const string RecordEvaluationMethodName = "RecordEvaluation";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [VKDiagnosticDescriptors.MissingObservabilityMetrics, VKDiagnosticDescriptors.MissingStopwatchUsage];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Check classes that implement IAuthorizationHandler or have [Diagnosable] attribute
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        if (namedTypeSymbol.IsAbstract || namedTypeSymbol.TypeKind != TypeKind.Class)
        {
            return;
        }

        // Rule: Check classes that implement IAuthorizationHandler OR have [Diagnosable]
        var isDiagnosable = namedTypeSymbol.ImplementsInterface(AuthorizationHandlerInterface) ||
                           namedTypeSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == DiagnosableAttribute);

        if (!isDiagnosable)
        {
            return;
        }

        // Search for RecordProcess/RecordEvaluation and Stopwatch usage within the class methods
        var hasDiagnosticRecordCall = false;
        var hasStopwatchUsage = false;

        foreach (var member in namedTypeSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            if (member.IsAbstract)
            {
                continue;
            }

            var syntaxRefs = member.DeclaringSyntaxReferences;
            foreach (var syntaxRef in syntaxRefs)
            {
                var syntax = syntaxRef.GetSyntax();
                var descendantNodes = syntax.DescendantNodes().ToList();

                var invocations = descendantNodes.OfType<InvocationExpressionSyntax>();
                foreach (var invocation in invocations)
                {
                    var name = GetMethodName(invocation);
                    if (name == RecordProcessMethodName || name == RecordEvaluationMethodName)
                    {
                        hasDiagnosticRecordCall = true;
                    }
                    else if (name == "StartNew" && invocation.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "Stopwatch" } })
                    {
                        hasStopwatchUsage = true;
                    }
                }

                if (!hasStopwatchUsage)
                {
                    // Check for new Stopwatch()
                    hasStopwatchUsage = descendantNodes.OfType<ObjectCreationExpressionSyntax>()
                        .Any(oc => oc.Type is IdentifierNameSyntax { Identifier.Text: "Stopwatch" });
                }
            }
        }

        if (!hasDiagnosticRecordCall)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                VKDiagnosticDescriptors.MissingObservabilityMetrics,
                namedTypeSymbol.Locations[0],
                namedTypeSymbol.Name));
        }

        if (!hasStopwatchUsage)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                VKDiagnosticDescriptors.MissingStopwatchUsage,
                namedTypeSymbol.Locations[0],
                namedTypeSymbol.Name));
        }
    }

    private static string? GetMethodName(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is IdentifierNameSyntax identifier)
        {
            return identifier.Identifier.Text;
        }

        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Name.Identifier.Text;
        }

        return null;
    }
}


