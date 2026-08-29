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

namespace VK.Tools.SourceGenerators.CodeFixes.Persist;

/// <summary>
/// Code Fix Provider that automatically inserts missing persistence registrations
/// (AddGeneratedPersistenceRepositories and AddGeneratedModelContributors) into the DI setup method.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VKPersistRegistrationCodeFixProvider)), Shared]
public sealed class VKPersistRegistrationCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
        => [
            VKDiagnosticDescriptors.MissingAggregateRepositoriesRegistration.Id,
            VKDiagnosticDescriptors.MissingModelContributorsRegistration.Id
        ];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        // Find candidate method with IServiceCollection parameter in the current document (e.g. AddServices, RegisterBlockCustom, Program)
        var candidateMethod = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.ParameterList.Parameters.Any(p =>
                p.Type?.ToString().Contains("IServiceCollection") == true ||
                p.Type?.ToString().Contains("Builder") == true));

        if (candidateMethod?.Body is null) return;

        foreach (var diagnostic in context.Diagnostics)
        {
            if (diagnostic.Id == VKDiagnosticDescriptors.MissingAggregateRepositoriesRegistration.Id)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Register generated aggregate repositories (AddGeneratedAggregateRepositories)",
                        createChangedDocument: c => AddInvocationAsync(context.Document, candidateMethod, "services.AddGeneratedAggregateRepositories();", c),
                        equivalenceKey: "AddAggregateRepositories"),
                    diagnostic);
            }
            else if (diagnostic.Id == VKDiagnosticDescriptors.MissingModelContributorsRegistration.Id)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Register generated model & convention contributors (AddGeneratedModelContributors)",
                        createChangedDocument: c => AddInvocationAsync(context.Document, candidateMethod, "services.AddGeneratedModelContributors();", c),
                        equivalenceKey: "AddModelContributors"),
                    diagnostic);
            }
        }
    }

    private static async Task<Document> AddInvocationAsync(
        Document document,
        MethodDeclarationSyntax method,
        string statementText,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null || method.Body is null) return document;

        var newStatement = SyntaxFactory.ParseStatement(statementText + "\n")
            .WithLeadingTrivia(SyntaxFactory.Whitespace("        "));

        var newBody = method.Body.AddStatements(newStatement);
        var newRoot = root.ReplaceNode(method.Body, newBody);

        return document.WithSyntaxRoot(newRoot);
    }
}
