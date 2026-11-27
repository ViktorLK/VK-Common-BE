using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace VK.Tools.Analyzers;

/// <summary>
/// Enforces AP.01 property initialization discipline by prohibiting 'default!' on property declarations.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoDefaultBangPropertyAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(VKAnalyzerDescriptors.DefaultBangPropertyInitializer);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var property = (PropertyDeclarationSyntax)context.Node;
        var initializerValue = property.Initializer?.Value;

        if (initializerValue is PostfixUnaryExpressionSyntax postfix &&
            postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression) &&
            (postfix.Operand.IsKind(SyntaxKind.DefaultLiteralExpression) ||
             postfix.Operand is DefaultExpressionSyntax))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                VKAnalyzerDescriptors.DefaultBangPropertyInitializer,
                postfix.GetLocation(),
                property.Identifier.ValueText));
        }
    }
}
