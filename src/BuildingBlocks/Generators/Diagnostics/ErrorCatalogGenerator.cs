using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Blocks.Generators.Diagnostics.Internal;
using VK.Blocks.Generators.Extensions;
using VK.Blocks.Generators.Utilities;

namespace VK.Blocks.Generators.Diagnostics;


/// <summary>
/// Scans for all static readonly Error fields in the module and generates an ErrorCatalog.
/// This allows for centralized discovery, documentation, and i18n support.
/// </summary>
[Generator]
public sealed class ErrorCatalogGenerator : IIncrementalGenerator
{
    private const string ErrorTypeName = "Error";
    private const string ErrorFullTypeName = VKBlocksConstants.VKBlocksPrefix + "Core.Results.Error";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var errorFields = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is FieldDeclarationSyntax field && IsCandidate(field),
                transform: GetErrorInfo)
            .Where(x => x is not null);

        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var combined = errorFields.Collect().Combine(assemblyName);

        context.RegisterSourceOutput(combined, (spc, pair) => Execute(spc, pair.Left, pair.Right, GetType()));
    }

    private static bool IsCandidate(FieldDeclarationSyntax field)
    {
        // Must be public/internal and static readonly
        var modifiers = field.Modifiers.ToString();
        if (!modifiers.Contains("static") || !modifiers.Contains("readonly"))
        {
            return false;
        }

        return field.Declaration.Type.ToString().EndsWith(ErrorTypeName);
    }

    private static ErrorInfo? GetErrorInfo(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        var variable = fieldDeclaration.Declaration.Variables.FirstOrDefault();
        if (variable is null)
        {
            return null;
        }

        if (context.SemanticModel.GetDeclaredSymbol(variable, ct) is not IFieldSymbol symbol || symbol.Type.ToDisplayString() != ErrorFullTypeName)
        {
            return null;
        }

        // Try to extract metadata from the initializer (new Error("Code", "Description", ...))
        string? code = null;
        string? description = null;
        var type = "Failure";

        if (variable.Initializer?.Value is ObjectCreationExpressionSyntax creation)
        {
            var argumentList = creation.ArgumentList;
            if (argumentList != null)
            {
                var args = argumentList.Arguments;
                if (args.Count > 0 && context.SemanticModel.GetConstantValue(args[0].Expression, ct).Value is string c)
                {
                    code = c;
                }
                if (args.Count > 1 && context.SemanticModel.GetConstantValue(args[1].Expression, ct).Value is string d)
                {
                    description = d;
                }
                if (args.Count > 2)
                {
                    // ErrorType is an enum, try to get its name
                    var typeSymbol = context.SemanticModel.GetSymbolInfo(args[2].Expression, ct).Symbol;
                    if (typeSymbol != null)
                    {
                        type = typeSymbol.Name;
                    }
                }
            }
        }
        else if (variable.Initializer?.Value is InvocationExpressionSyntax invocation)
        {
            // Factory method like Error.Validation("Code", "Description")
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                type = memberAccess.Name.Identifier.Text;
                var argumentList = invocation.ArgumentList;
                if (argumentList != null)
                {
                    var args = argumentList.Arguments;
                    if (args.Count > 0 && context.SemanticModel.GetConstantValue(args[0].Expression, ct).Value is string c)
                    {
                        code = c;
                    }
                    if (args.Count > 1 && context.SemanticModel.GetConstantValue(args[1].Expression, ct).Value is string d)
                    {
                        description = d;
                    }
                }
            }
        }

        return new ErrorInfo(
            SymbolName: symbol.Name,
            FullTypeName: symbol.ContainingType.ToDisplayString(),
            Code: code ?? symbol.Name,
            Description: description ?? "Defined in " + symbol.ContainingType.Name,
            ErrorType: type);
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<ErrorInfo?> errors, string? assemblyName, Type generatorType)
    {
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
        {
            return;
        }

        var validErrors = errors.Where(e => e is not null).Select(e => e!).OrderBy(e => e.Code).ToList();
        if (validErrors.Count == 0)
        {
            return;
        }

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using VK.Blocks.Core.Results;");
        sb.AppendLine();

        // Use the base namespace of the module (e.g., VK.Blocks.Authentication)
        var rootNamespace = assemblyName?.Replace(".Core", "").Replace(".OpenIdConnect", "");
        sb.AppendLine($"namespace {rootNamespace}.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Central catalog of all Errors defined in this module.");
        sb.AppendLine("    /// Used for discovery, diagnostics, and i18n mapping.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static class ErrorCatalog");
        sb.AppendLine("    {");
        sb.AppendLine("        /// <summary>Deterministic hash of the error catalog.</summary>");
        sb.AppendLine($"        public const string MetadataHash = \"{CalculateDeterministicHash(validErrors)}\";");
        sb.AppendLine();
        sb.AppendLine("        /// <summary>A complete list of all Errors defined in this module.</summary>");
        sb.AppendLine("        public static readonly IReadOnlyList<Error> All = new List<Error>");
        sb.AppendLine("        {");

        foreach (var e in validErrors)
        {
            sb.AppendLine($"            {e.FullTypeName}.{e.SymbolName},");
        }

        sb.AppendLine("        }.AsReadOnly();");
        sb.AppendLine();
        sb.AppendLine("        /// <summary>Maps error codes to their standardized Error objects.</summary>");
        sb.AppendLine("        public static readonly IReadOnlyDictionary<string, Error> Map = new Dictionary<string, Error>");
        sb.AppendLine("        {");
        foreach (var e in validErrors)
        {
            sb.AppendLine($"            [\"{e.Code}\"] = {e.FullTypeName}.{e.SymbolName},");
        }
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource("ErrorCatalog.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static string CalculateDeterministicHash(IEnumerable<ErrorInfo> errors)
    {
        var hash = 14695981039346656037UL;
        foreach (var e in errors)
        {
            hash = Fnv1aHash.Compute(e.Code, hash);
            hash = Fnv1aHash.Compute(e.Description, hash);
        }
        return hash.ToString("X16");
    }
}
