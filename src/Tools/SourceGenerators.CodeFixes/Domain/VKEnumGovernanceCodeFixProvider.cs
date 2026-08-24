using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Diagnostics;

namespace VK.Tools.SourceGenerators.CodeFixes.Domain;

/// <summary>
/// Code fix provider for VK1101 and VK1102 enum governance rules.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VKEnumGovernanceCodeFixProvider)), Shared]
public sealed class VKEnumGovernanceCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
        => [VKDiagnosticDescriptors.EnumMemberMustHaveExplicitValue.Id, VKDiagnosticDescriptors.EnumMustDeclareUnderlyingType.Id];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            if (diagnostic.Id == VKDiagnosticDescriptors.EnumMustDeclareUnderlyingType.Id)
            {
                var enumDeclaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
                    .OfType<EnumDeclarationSyntax>()
                    .FirstOrDefault();

                if (enumDeclaration is not null)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Declare ': byte' underlying type",
                            createChangedDocument: c => AddUnderlyingTypeAsync(context.Document, enumDeclaration, "byte", c),
                            equivalenceKey: "AddUnderlyingTypeByte"),
                        diagnostic);

                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Declare ': short' underlying type",
                            createChangedDocument: c => AddUnderlyingTypeAsync(context.Document, enumDeclaration, "short", c),
                            equivalenceKey: "AddUnderlyingTypeShort"),
                        diagnostic);
                }
            }
            else if (diagnostic.Id == VKDiagnosticDescriptors.EnumMemberMustHaveExplicitValue.Id)
            {
                var memberDeclaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
                    .OfType<EnumMemberDeclarationSyntax>()
                    .FirstOrDefault();

                if (memberDeclaration is not null && memberDeclaration.Parent is EnumDeclarationSyntax enumDecl)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Add explicit enum value assignment",
                            createChangedDocument: c => AddExplicitValueAsync(context.Document, enumDecl, memberDeclaration, c),
                            equivalenceKey: "AddExplicitEnumValue"),
                        diagnostic);
                }
            }
        }
    }

    private static async Task<Document> AddUnderlyingTypeAsync(Document document, EnumDeclarationSyntax enumDeclaration, string typeName, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var baseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(typeName));
        var baseList = SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(baseType));

        var newEnumDeclaration = enumDeclaration.WithBaseList(baseList);
        var newRoot = root.ReplaceNode(enumDeclaration, newEnumDeclaration);
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> AddExplicitValueAsync(Document document, EnumDeclarationSyntax enumDeclaration, EnumMemberDeclarationSyntax targetMember, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // Determine next assigned value based on preceding members
        long nextValue = 0;
        foreach (var member in enumDeclaration.Members)
        {
            if (member == targetMember)
            {
                break;
            }

            if (member.EqualsValue?.Value is LiteralExpressionSyntax literal &&
                long.TryParse(literal.Token.ValueText, out var val))
            {
                nextValue = val + 1;
            }
            else
            {
                nextValue++;
            }
        }

        var equalsValue = SyntaxFactory.EqualsValueClause(
            SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(nextValue)));

        var newMember = targetMember.WithEqualsValue(equalsValue);
        var newRoot = root.ReplaceNode(targetMember, newMember);
        return document.WithSyntaxRoot(newRoot);
    }
}
