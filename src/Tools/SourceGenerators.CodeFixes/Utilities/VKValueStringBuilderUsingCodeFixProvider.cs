using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VK.Tools.SourceGenerators.CodeFixes.Utilities;

/// <summary>
/// Provides a Quick-Fix CodeFix Provider for VKCORE001: adding 'using' keyword to local VKValueStringBuilder declarations.
/// </summary>

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VKValueStringBuilderUsingCodeFixProvider)), Shared]
public sealed class VKValueStringBuilderUsingCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("VKCORE001");

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var token = root.FindToken(diagnosticSpan.Start);
        var localDeclaration = token.Parent?.FirstAncestorOrSelf<LocalDeclarationStatementSyntax>();

        if (localDeclaration is not null)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add 'using' declaration",
                    createChangedDocument: c => AddUsingKeywordAsync(context.Document, root, localDeclaration, c),
                    equivalenceKey: nameof(VKValueStringBuilderUsingCodeFixProvider)),
                diagnostic);
        }
    }

    private static Task<Document> AddUsingKeywordAsync(
        Document document,
        SyntaxNode root,
        LocalDeclarationStatementSyntax localDeclaration,
        CancellationToken cancellationToken)
    {
        var newLocalDeclaration = localDeclaration.WithUsingKeyword(SyntaxFactory.Token(SyntaxKind.UsingKeyword).WithTrailingTrivia(SyntaxFactory.Space));
        var newRoot = root.ReplaceNode(localDeclaration, newLocalDeclaration);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}
