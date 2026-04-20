using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Blocks.Generators.Authorization.Internal;
using VK.Blocks.Generators.Extensions;
using VK.Blocks.Generators.Utilities;

namespace VK.Blocks.Generators.Authorization;

/// <summary>
/// Source generator that builds a compile-time authorization metadata map by scanning
/// for classes and methods decorated with IAuthorizationRequirementData attributes.
/// </summary>
[Generator]
public sealed class AuthorizationMetadataGenerator : IIncrementalGenerator
{
    private const string RequirementDataInterfaceName = "IAuthorizationRequirementData";
    private const string AuthorizeAttributeName = "AuthorizeAttribute";
    private const string PermissionAttributeName = "AuthorizePermissionAttribute";
    private const string RolesAttributeName = "AuthorizeRolesAttribute";
    private const string DynamicAttributeName = "DynamicAuthorizeAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Only generate for assemblies that reference VK.Blocks.Authorization
        var assemblyName = context.CompilationProvider.Select(
            static (c, _) => c.AssemblyName);

        // 1. Find all candidate attributes that might implement IAuthorizationRequirementData
        var endpointMetadata = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: IsCandidateForAuthorizationMetadata,
                transform: GetAuthorizationMetadata)
            .WhereNotNull();

        // 2. Combine and generate
        var combined = endpointMetadata.Collect().Combine(assemblyName);
        context.RegisterSourceOutput(combined, (spc, pair) => Execute(spc, pair.Left, pair.Right, this.GetType()));
    }

    private static bool IsCandidateForAuthorizationMetadata(SyntaxNode node, System.Threading.CancellationToken _)
    {
        // We are interested in Classes (Controllers) and Methods (Actions) that have attributes
        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } ||
               node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private static EndpointMetadata? GetAuthorizationMetadata(GeneratorSyntaxContext context, System.Threading.CancellationToken ct)
    {
        var node = context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(node, ct);

        if (symbol is null)
        {
            return null;
        }

        var attributes = symbol.GetAttributes();
        var authAttributes = attributes
            .Where(a => a.AttributeClass.ImplementsInterface(RequirementDataInterfaceName))
            .ToList();

        if (authAttributes.Count == 0)
        {
            return null;
        }

        var permissions = new List<string>();
        var roles = new List<string>();
        string? rank = null;
        var requiresInternal = false;
        var requiresWorkingHours = false;

        foreach (var attr in authAttributes)
        {
            var name = attr.AttributeClass?.Name;

            // Handle Permissions
            if (name?.Contains("Permission") == true)
            {
                // AuthorizePermissionAttribute has a constructor with permission string
                if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string p)
                {
                    permissions.Add(p);
                }
                else
                {
                    // Check for PermissionMetadataAttribute on the attribute class (for generated attributes)
                    var metadata = attr.AttributeClass?.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.Name.Contains("PermissionMetadata") == true);

                    if (metadata?.ConstructorArguments.Length > 0 && metadata.ConstructorArguments[0].Value is string p2)
                    {
                        permissions.Add(p2);
                    }
                }
            }
            // Handle Roles
            else if (name?.Contains("Roles") == true)
            {
                if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Values is var roleValues)
                {
                    roles.AddRange(roleValues.Select(v => v.Value?.ToString() ?? "").Where(s => !string.IsNullOrEmpty(s)));
                }
            }
            // Handle Rank
            else if (name?.Contains("Rank") == true)
            {
                if (attr.ConstructorArguments.Length > 0)
                {
                    rank = attr.ConstructorArguments[0].Value?.ToString();
                }
            }
            // Handle Infrastructure Requirements (InternalNetwork / WorkingHours)
            else if (name?.Contains("InternalNetwork") == true)
            {
                requiresInternal = true;
            }
            else if (name?.Contains("WorkingHours") == true)
            {
                requiresWorkingHours = true;
            }
        }

        return new EndpointMetadata(
            EndpointName: symbol.ToDisplayString(),
            Permissions: [.. permissions],
            Roles: [.. roles],
            MinimumRank: rank,
            RequiresInternalNetwork: requiresInternal,
            RequiresWorkingHours: requiresWorkingHours);
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
                    Permissions: [.. g.SelectMany(m => m.Permissions).Distinct()],
                    Roles: [.. g.SelectMany(m => m.Roles).Distinct()],
                    MinimumRank: g.Select(m => m.MinimumRank).FirstOrDefault(r => r != null),
                    RequiresInternalNetwork: g.Any(m => m.RequiresInternalNetwork),
                    RequiresWorkingHours: g.Any(m => m.RequiresWorkingHours)
                );
            })
            .OrderBy(m => m.EndpointName)
            .ToList();

        var hash = CalculateDeterministicHash(mergedMetadata);

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using VK.Blocks.Authorization.Common;");
        sb.AppendLine();
        sb.AppendLine("namespace VK.Blocks.Authorization.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Provides a compile-time map of all authorization requirements for discovery and diagnostics.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static class AuthorizationMetadata");
        sb.AppendLine("    {");
        sb.AppendLine("        /// <summary>Deterministic hash of the authorization topology.</summary>");
        sb.AppendLine($"        public const string MetadataHash = \"{hash}\";");
        sb.AppendLine();
        sb.AppendLine("        /// <summary>Maps endpoint display names to their derived authorization information.</summary>");
        sb.AppendLine("        public static readonly IReadOnlyDictionary<string, EndpointAuthorizationInfo> Endpoints = new Dictionary<string, EndpointAuthorizationInfo>");
        sb.AppendLine("        {");

        foreach (var m in mergedMetadata)
        {
            sb.AppendLine($"            [\"{m.EndpointName}\"] = new EndpointAuthorizationInfo");
            sb.AppendLine("            {");
            sb.AppendLine($"                EndpointName = \"{m.EndpointName}\",");
            sb.Append("                Permissions = new string[] { ");
            sb.Append(string.Join(", ", m.Permissions.Select(p => $"\"{p}\"")));
            sb.AppendLine(" },");
            sb.Append("                Roles = new string[] { ");
            sb.Append(string.Join(", ", m.Roles.Select(r => $"\"{r}\"")));
            sb.AppendLine(" },");
            sb.AppendLine($"                MinimumRank = {(m.MinimumRank != null ? $"\"{m.MinimumRank}\"" : "null")},");
            sb.AppendLine($"                RequiresInternalNetwork = {m.RequiresInternalNetwork.ToString().ToLowerInvariant()},");
            sb.AppendLine($"                RequiresWorkingHours = {m.RequiresWorkingHours.ToString().ToLowerInvariant()}");
            sb.AppendLine("            },");
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource("AuthorizationMetadata.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static string CalculateDeterministicHash(IEnumerable<EndpointMetadata> metadata)
    {
        var hash = 14695981039346656037UL; // Initial seed
        foreach (var m in metadata)
        {
            hash = Fnv1aHash.Compute(m.EndpointName, hash);
            foreach (var p in m.Permissions)
            {
                hash = Fnv1aHash.Compute(p, hash);
            }

            foreach (var r in m.Roles)
            {
                hash = Fnv1aHash.Compute(r, hash);
            }

            hash = Fnv1aHash.Compute(m.MinimumRank ?? "", hash);
            hash = Fnv1aHash.Compute(m.RequiresInternalNetwork.ToString(), hash);
            hash = Fnv1aHash.Compute(m.RequiresWorkingHours.ToString(), hash);
        }
        return hash.ToString("X16");
    }


}
