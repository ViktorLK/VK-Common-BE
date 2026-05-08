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
using Microsoft.CodeAnalysis.Editing;
using VK.Tools.SourceGenerators.Diagnostics;
using VK.Tools.SourceGenerators.Utilities;


namespace VK.Tools.SourceGenerators.CodeFixes.Observability;

/// <summary>
/// CodeFixProvider that automatically injects OR.01 compliant observability code.
/// Resolves VK1001 (Missing RecordEvaluation) and VK1002 (Missing Stopwatch usage).
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ObservabilityCodeFixProvider)), Shared]
public sealed class ObservabilityCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
        => [VKDiagnosticDescriptors.MissingObservabilityMetrics.Id, VKDiagnosticDescriptors.MissingStopwatchUsage.Id];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the method declaration containing the diagnostic
            var methodDeclaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();

            if (methodDeclaration != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Add VK.Blocks observability pattern (OR.01)",
                        createChangedDocument: c => AddObservabilityAsync(context.Document, methodDeclaration, c),
                        equivalenceKey: nameof(ObservabilityCodeFixProvider)),
                    diagnostic);
            }
        }
    }

    /// <summary>
    /// Performs the actual syntax transformation to inject observability snippets.
    /// </summary>
    /// <param name="document">The source document.</param>
    /// <param name="methodDeclaration">The target method declaration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The modified document.</returns>
    private static async Task<Document> AddObservabilityAsync(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var generator = editor.Generator;

        // 1. Prepare standard diagnostics pieces
        var stopwatchStatement = generator.LocalDeclarationStatement(
            "sw",
            generator.InvocationExpression(
                generator.MemberAccessExpression(
                    generator.IdentifierName("Stopwatch"),
                    "StartNew")));

        var actionNameArgument = generator.InvocationExpression(
            generator.IdentifierName("nameof"),
            generator.IdentifierName(methodDeclaration.Identifier.Text));

        // 2. Identify the 'result' variable or use default(VKResult) placeholder
        var hasResultVariable = methodDeclaration.DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Any(v => v.Identifier.Text == "result" || v.Identifier.Text == "res");

        var resultIdentifier = hasResultVariable ? "result" : "res"; // Prefer existing variable names
        if (!hasResultVariable)
        {
            // Check if there's any assignment to a variable named 'result' or 'res'
            var hasResultAssignment = methodDeclaration.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Any(a => a.Left is IdentifierNameSyntax id && (id.Identifier.Text == "result" || id.Identifier.Text == "res"));
            hasResultVariable = hasResultAssignment;
        }

        var resultArg = hasResultVariable
            ? generator.IdentifierName("result")
            : generator.DefaultExpression(generator.IdentifierName("VKResult"));

        var recordCall = generator.ExpressionStatement(
            generator.InvocationExpression(
                generator.MemberAccessExpression(generator.IdentifierName("sw"), "RecordProcess"),
                actionNameArgument,
                resultArg));

        // 3. Handle Method Body
        var body = methodDeclaration.Body;
        if (body == null || body.Statements.Count == 0)
        {
            // Inject full template for empty methods
            var placeholderResult = (StatementSyntax)generator.LocalDeclarationStatement("result",
                generator.DefaultExpression(generator.IdentifierName("VKResult")));

            var fixedRecordCall = (StatementSyntax)generator.ExpressionStatement(
                generator.InvocationExpression(
                    generator.MemberAccessExpression(generator.IdentifierName("sw"), "RecordProcess"),
                    actionNameArgument,
                    generator.IdentifierName("result")));

            var statements = SyntaxFactory.List<StatementSyntax>(
            [
                (StatementSyntax)stopwatchStatement,
                placeholderResult,
                fixedRecordCall,
                SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("result"))
            ]);

            var newBody = SyntaxFactory.Block(statements);
            editor.ReplaceNode(methodDeclaration, methodDeclaration.WithBody(newBody).WithExpressionBody(null).WithSemicolonToken(default));
        }
        else
        {
            // Case: Method has statements. Inject pieces carefully.

            // Add Stopwatch if missing
            var hasStopwatch = methodDeclaration.DescendantNodes()
                .OfType<VariableDeclaratorSyntax>()
                .Any(v => v.Identifier.Text == "sw");

            if (!hasStopwatch)
            {
                editor.InsertBefore(body.Statements.First(), stopwatchStatement);
            }

            // Add RecordProcess if missing
            var hasRecordCall = methodDeclaration.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(i => i.Expression is MemberAccessExpressionSyntax ma &&
                         (ma.Name.Identifier.Text == "RecordProcess" || ma.Name.Identifier.Text == "RecordEvaluation"));

            if (!hasRecordCall)
            {
                var lastStatement = body.Statements.Last();
                if (lastStatement is ReturnStatementSyntax)
                {
                    editor.InsertBefore(lastStatement, recordCall);
                }
                else
                {
                    editor.InsertAfter(lastStatement, recordCall);
                }
            }
        }

        // 4. Handle Usings
        var root = (CompilationUnitSyntax?)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root != null)
        {
            var usings = root.Usings;
            var systemDiagnostics = "System.Diagnostics";
            var vkCoreResults = $"{VKBlocksConstants.VKBlocksPrefix}.Core";

            var newUsings = usings;
            if (!usings.Any(u => u.Name?.ToString() == systemDiagnostics))
            {
                newUsings = newUsings.Add((UsingDirectiveSyntax)generator.NamespaceImportDeclaration(systemDiagnostics));
            }
            if (!usings.Any(u => u.Name?.ToString() == vkCoreResults))
            {
                newUsings = newUsings.Add((UsingDirectiveSyntax)generator.NamespaceImportDeclaration(vkCoreResults));
            }

            if (newUsings != usings)
            {
                editor.ReplaceNode(root, root.WithUsings(newUsings));
            }
        }

        return editor.GetChangedDocument();
    }
}

