using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace VK.Tools.SourceGenerators.Utilities;

/// <summary>
/// Roslyn Diagnostic Analyzer enforcing that VKValueStringBuilder instances are declared with a 'using' statement or declaration.
/// Enforces Error severity (VKCORE001) to prevent ArrayPool memory leaks.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class VKValueStringBuilderUsingAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "VKCORE001";
    private const string Category = "MemoryUsage";

    private static readonly LocalizableString Title = "VKValueStringBuilder must be declared with using";
    private static readonly LocalizableString MessageFormat = "'VKValueStringBuilder' instance '{0}' must be declared with 'using' to ensure rented ArrayPool buffers are returned";
    private static readonly LocalizableString Description = "VKValueStringBuilder rents memory buffers from ArrayPool when capacity exceeds initial stack span. Using 'using' guarantees buffer release.";

    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeLocalDeclaration, SyntaxKind.LocalDeclarationStatement);
    }

    private static void AnalyzeLocalDeclaration(SyntaxNodeAnalysisContext context)
    {
        var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;

        // If it already has a using keyword (using var sb = ... or using (var sb = ...)), it's compliant
        if (localDeclaration.UsingKeyword != default && !localDeclaration.UsingKeyword.IsKind(SyntaxKind.None))
        {
            return;
        }

        var variableType = context.SemanticModel.GetTypeInfo(localDeclaration.Declaration.Type).Type;
        if (variableType is null) return;

        if (variableType.ToDisplayString() == "VK.Blocks.Core.VKValueStringBuilder" ||
            variableType.Name == "VKValueStringBuilder")
        {
            foreach (var variable in localDeclaration.Declaration.Variables)
            {
                var diagnostic = Diagnostic.Create(Rule, variable.Identifier.GetLocation(), variable.Identifier.ValueText);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
