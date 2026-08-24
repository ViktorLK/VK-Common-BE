using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using VK.Tools.SourceGenerators.Diagnostics;

namespace VK.Tools.SourceGenerators.Domain;

/// <summary>
/// Analyzer enforcing industrial enum governance:
/// 1. VK1101: All enum members must have explicit integer values.
/// 2. VK1102: All enums must explicitly declare an underlying type (e.g., : byte, : short).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class VKEnumGovernanceAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [VKDiagnosticDescriptors.EnumMemberMustHaveExplicitValue, VKDiagnosticDescriptors.EnumMustDeclareUnderlyingType];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeEnumDeclaration, SyntaxKind.EnumDeclaration);
    }

    private static void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
    {
        var enumDeclaration = (EnumDeclarationSyntax)context.Node;

        // 1. Check underlying type (VK1102)
        if (enumDeclaration.BaseList is null || enumDeclaration.BaseList.Types.Count == 0)
        {
            var diagnostic = Diagnostic.Create(
                VKDiagnosticDescriptors.EnumMustDeclareUnderlyingType,
                enumDeclaration.Identifier.GetLocation(),
                enumDeclaration.Identifier.Text);

            context.ReportDiagnostic(diagnostic);
        }

        // 2. Check explicit values for each member (VK1101)
        foreach (var member in enumDeclaration.Members)
        {
            if (member.EqualsValue is null)
            {
                var diagnostic = Diagnostic.Create(
                    VKDiagnosticDescriptors.EnumMemberMustHaveExplicitValue,
                    member.Identifier.GetLocation(),
                    member.Identifier.Text);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
