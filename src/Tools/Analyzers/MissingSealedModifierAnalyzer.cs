using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace VK.Tools.Analyzers;

/// <summary>
/// Enforces AP.01 default-sealed class discipline for non-abstract classes not designed for inheritance.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingSealedModifierAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(VKAnalyzerDescriptors.MissingSealedModifier);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        if (namedTypeSymbol.TypeKind != TypeKind.Class ||
            namedTypeSymbol.IsSealed ||
            namedTypeSymbol.IsAbstract ||
            namedTypeSymbol.IsStatic ||
            namedTypeSymbol.IsImplicitlyDeclared ||
            namedTypeSymbol.Locations.Length == 0)
        {
            return;
        }

        if (DerivesFromException(namedTypeSymbol) || HasInheritanceDesignMarkers(namedTypeSymbol))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            VKAnalyzerDescriptors.MissingSealedModifier,
            namedTypeSymbol.Locations[0],
            namedTypeSymbol.Name);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool DerivesFromException(INamedTypeSymbol typeSymbol)
    {
        var current = typeSymbol.BaseType;
        while (current is not null)
        {
            if (current.ToDisplayString() == "System.Exception")
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static bool HasInheritanceDesignMarkers(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member.IsImplicitlyDeclared)
            {
                continue;
            }

            if (member.IsVirtual ||
                member.DeclaredAccessibility is Accessibility.Protected
                    or Accessibility.ProtectedOrInternal
                    or Accessibility.ProtectedAndInternal)
            {
                return true;
            }
        }

        return false;
    }
}
