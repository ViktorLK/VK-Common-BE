using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Blocks.Generators.Authentication.Internal;
using VK.Blocks.Generators.Extensions;
using VK.Blocks.Generators.Utilities;

namespace VK.Blocks.Generators.Authentication;

/// <summary>
/// Source generator that builds a compile-time authentication metadata map by scanning
/// for classes and methods decorated with authorization attributes.
/// </summary>
[Generator]
public sealed class AuthenticationMetadataGenerator : IIncrementalGenerator
{
    private const string AuthorizeAttributeName = "AuthorizeAttribute";
    private const string AllowAnonymousAttributeName = "AllowAnonymousAttribute";
    private const string AuthGroupAttributeName = "AuthGroupAttribute";
    private const string AuthGroupMetadataAttributeName = VKBlocksConstants.VKBlocksPrefix + "Authentication.Features.SemanticAttributes.Metadata.AuthGroupMetadataAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Only generate for assemblies that reference VK.Blocks.Authentication
        var assemblyName = context.CompilationProvider.Select(
            static (c, _) => c.AssemblyName);

        var endpointMetadata = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: IsCandidateForAuthenticationMetadata,
                transform: GetAuthenticationMetadata)
            .WhereNotNull();

        var combined = endpointMetadata.Collect().Combine(assemblyName);

        context.RegisterSourceOutput(combined, (spc, pair) => Execute(spc, pair.Left, pair.Right, this.GetType()));
    }

    private static bool IsCandidateForAuthenticationMetadata(SyntaxNode node, System.Threading.CancellationToken _)
    {
        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } ||
               node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private static EndpointMetadata? GetAuthenticationMetadata(GeneratorSyntaxContext context, System.Threading.CancellationToken ct)
    {
        var node = context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(node, ct);

        if (symbol is null)
        {
            return null;
        }

        var attributes = symbol.GetAttributes();

        // We look for AuthorizeAttribute (and its subclasses) and AllowAnonymousAttribute
        var authAttributes = attributes
            .Where(a => a.AttributeClass?.InheritsFromOrIs(AuthorizeAttributeName) == true ||
                        a.AttributeClass?.Name == AllowAnonymousAttributeName)
            .ToList();

        if (authAttributes.Count == 0)
        {
            return null;
        }

        var policies = new List<string>();
        var schemes = new List<string>();
        string? authGroup = null;
        var isAnonymous = false;

        foreach (var attr in authAttributes)
        {
            var className = attr.AttributeClass?.Name;

            if (className == AllowAnonymousAttributeName)
            {
                isAnonymous = true;
                continue;
            }

            // Extract AuthGroup from AuthGroupAttribute or Metadata (Typed Attributes)
            if (className == AuthGroupAttributeName)
            {
                if (attr.ConstructorArguments.Length > 0)
                {
                    authGroup = attr.ConstructorArguments[0].Value?.ToString();
                }
            }
            else
            {
                // Check for AuthGroupMetadataAttribute on the attribute class (for typed attributes)
                var metadata = attr.AttributeClass?.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == AuthGroupMetadataAttributeName);

                if (metadata?.ConstructorArguments.Length > 0)
                {
                    authGroup = metadata.ConstructorArguments[0].Value?.ToString();
                }
            }

            // Extract Policy
            if (attr.NamedArguments.Any(na => na.Key == "Policy"))
            {
                var policyVal = attr.NamedArguments.First(na => na.Key == "Policy").Value.Value?.ToString();
                if (!string.IsNullOrEmpty(policyVal))
                {
                    policies.Add(policyVal!);
                }
            }
            else if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string policyCtor)
            {
                // Standard AuthorizeAttribute(policy)
                policies.Add(policyCtor);
            }

            // Extract Schemes
            if (attr.NamedArguments.Any(na => na.Key == "AuthenticationSchemes"))
            {
                var schemesVal = attr.NamedArguments.First(na => na.Key == "AuthenticationSchemes").Value.Value?.ToString();
                if (!string.IsNullOrEmpty(schemesVal))
                {
                    schemes.AddRange(schemesVal!.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));
                }
            }
        }

        return new EndpointMetadata(
            EndpointName: symbol.ToDisplayString(),
            AuthGroup: authGroup,
            Schemes: [.. schemes.Distinct()],
            Policies: [.. policies.Distinct()],
            IsAnonymous: isAnonymous);
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<EndpointMetadata> metadata, string? assemblyName, Type generatorType)
    {
        // Guard against execution in unrelated assemblies (e.g., Core)
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
        {
            return;
        }

        // Group by endpoint name and merge (Class + Method attributes)
        var mergedMetadata = (metadata.IsDefaultOrEmpty ? Enumerable.Empty<EndpointMetadata>() : metadata)
            .GroupBy(m => m.EndpointName)
            .Select(g =>
            {
                return new EndpointMetadata(
                    EndpointName: g.Key,
                    AuthGroup: g.Select(m => m.AuthGroup).FirstOrDefault(r => r != null),
                    Schemes: [.. g.SelectMany(m => m.Schemes).Distinct()],
                    Policies: [.. g.SelectMany(m => m.Policies).Distinct()],
                    IsAnonymous: g.Any(m => m.IsAnonymous)
                );
            })
            .OrderBy(m => m.EndpointName)
            .ToList();

        var hash = CalculateDeterministicHash(mergedMetadata);

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using VK.Blocks.Authentication.Common;");
        sb.AppendLine();
        sb.AppendLine("namespace VK.Blocks.Authentication.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Provides a compile-time map of all authentication requirements for discovery and diagnostics.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    internal static class AuthenticationMetadata");
        sb.AppendLine("    {");
        sb.AppendLine("        /// <summary>Deterministic hash of the authentication topology.</summary>");
        sb.AppendLine($"        public const string MetadataHash = \"{hash}\";");
        sb.AppendLine();
        sb.AppendLine("        /// <summary>Maps endpoint display names to their derived authentication information.</summary>");
        sb.AppendLine("        public static readonly IReadOnlyDictionary<string, EndpointAuthenticationInfo> Endpoints = new Dictionary<string, EndpointAuthenticationInfo>");
        sb.AppendLine("        {");

        foreach (var m in mergedMetadata)
        {
            sb.AppendLine($"        [\"{m.EndpointName}\"] = new EndpointAuthenticationInfo");
            sb.AppendLine("        {");
            sb.AppendLine($"            EndpointName = \"{m.EndpointName}\",");
            sb.AppendLine($"            AuthGroup = {(m.AuthGroup != null ? $"\"{m.AuthGroup}\"" : "null")},");
            sb.Append("            Schemes = new string[] { ");
            sb.Append(string.Join(", ", m.Schemes.Select(s => $"\"{s}\"")));
            sb.AppendLine(" },");
            sb.Append("            Policies = new string[] { ");
            sb.Append(string.Join(", ", m.Policies.Select(p => $"\"{p}\"")));
            sb.AppendLine(" },");
            sb.AppendLine($"            IsAnonymous = {m.IsAnonymous.ToString().ToLowerInvariant()}");
            sb.AppendLine("        },");
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource("AuthenticationMetadata.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static string CalculateDeterministicHash(IEnumerable<EndpointMetadata> metadata)
    {
        var hash = 14695981039346656037UL; // Initial seed
        foreach (var m in metadata)
        {
            hash = Fnv1aHash.Compute(m.EndpointName, hash);
            hash = Fnv1aHash.Compute(m.AuthGroup ?? "", hash);
            foreach (var s in m.Schemes)
            {
                hash = Fnv1aHash.Compute(s, hash);
            }

            foreach (var p in m.Policies)
            {
                hash = Fnv1aHash.Compute(p, hash);
            }

            hash = Fnv1aHash.Compute(m.IsAnonymous.ToString(), hash);
        }
        return hash.ToString("X16");
    }

}
