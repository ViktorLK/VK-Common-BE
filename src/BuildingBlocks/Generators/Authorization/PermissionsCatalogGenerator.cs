using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Blocks.Generators.Extensions;

namespace VK.Blocks.Generators.Authorization;

/// <summary>
/// Scans for both permission usages ([AuthorizePermission]) and definitions ([GeneratePermissions])
/// to generate a central PermissionsCatalog and typed AuthorizeAttributes.
/// </summary>
[Generator]
public sealed class PermissionsCatalogGenerator : IIncrementalGenerator
{
    private const string UsageAttributeName = "AuthorizePermission";
    private const string DefinitionAttributeName = "GeneratePermissions";
    private const string RolesAttributeName = "AuthorizeRoles";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Source 1: Passive Discovery (Usage in Controllers/Methods)
        var passivePermissions = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is AttributeSyntax attr && IsMatchingAttribute(attr, UsageAttributeName, RolesAttributeName),
                transform: GetPermissionFromUsage)
            .Where(x => x is not null);

        // Source 2: Active Discovery (Definitions in marked Classes/Structs)
        var activePermissions = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is TypeDeclarationSyntax type && HasAttribute(type, DefinitionAttributeName),
                transform: GetPermissionsFromDefinition)
            .Where(x => x is not null)
            .SelectMany((x, _) => x!);

        // Combine both sources
        var combined = passivePermissions.Collect().Combine(activePermissions.Collect())
            .SelectMany((x, _) =>
            {
                var passiveList = x.Left;
                var activeList = x.Right;

                var merged = new Dictionary<string, PermissionInfo>();

                // 1. Add definitions first (Source of truth for metadata)
                foreach (var active in activeList)
                {
                    merged[active.Value] = active;
                }

                // 2. Add usages if not already defined
                foreach (var passive in passiveList)
                {
                    if (passive != null && !merged.ContainsKey(passive.Value))
                    {
                        merged[passive.Value] = passive;
                    }
                }

                return merged.Values;
            });

        context.RegisterSourceOutput(combined.Collect(), Execute);
    }

    private static bool IsMatchingAttribute(AttributeSyntax attr, params string[] targets)
    {
        var name = attr.Name.ToString();
        return targets.Any(t => name.EndsWith(t) || name.EndsWith(t + "Attribute"));
    }

    private static bool HasAttribute(TypeDeclarationSyntax type, string target)
    {
        return type.AttributeLists.Any(al => al.Attributes.Any(a =>
        {
            var name = a.Name.ToString();
            return name.EndsWith(target) || name.EndsWith(target + "Attribute");
        }));
    }

    private static PermissionInfo? GetPermissionFromUsage(GeneratorSyntaxContext context, System.Threading.CancellationToken ct)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;
        if (attributeSyntax.ArgumentList is null || attributeSyntax.ArgumentList.Arguments.Count == 0)
        {
            return null;
        }

        foreach (var argument in attributeSyntax.ArgumentList.Arguments)
        {
            var constantValue = context.SemanticModel.GetConstantValue(argument.Expression, ct);
            if (constantValue.HasValue && constantValue.Value is string val)
            {
                return new PermissionInfo(val, "Misc", null);
            }
        }
        return null;
    }

    private static IEnumerable<PermissionInfo> GetPermissionsFromDefinition(GeneratorSyntaxContext context, System.Threading.CancellationToken ct)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration, ct) is not INamedTypeSymbol typeSymbol)
        {
            yield break;
        }

        var moduleName = "Misc";
        var attribute = typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name.Contains(DefinitionAttributeName) == true);

        if (attribute is not null)
        {
            foreach (var namedArg in attribute.NamedArguments)
            {
                if (namedArg.Key == "Module" && namedArg.Value.Value is string m)
                {
                    moduleName = m;
                    break;
                }
            }
        }

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IFieldSymbol field && field.IsConst && field.ConstantValue is string val)
            {
                var displayName = field.Name;
                var description = "Auto-generated permission from code definition.";

                // Try to extract metadata from [Display] attribute
                var displayAttr = field.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.Name == "DisplayAttribute" || a.AttributeClass?.Name == "Display");

                if (displayAttr is not null)
                {
                    foreach (var namedArg in displayAttr.NamedArguments)
                    {
                        if (namedArg.Key == "Name" && namedArg.Value.Value is string n)
                        {
                            displayName = n;
                        }
                        else if (namedArg.Key == "Description" && namedArg.Value.Value is string d)
                        {
                            description = d;
                        }
                    }
                }

                yield return new PermissionInfo(val, moduleName, field.Name, displayName, description);
            }
        }
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<PermissionInfo> permissions)
    {
        if (permissions.IsDefaultOrEmpty)
        {
            return;
        }

        var uniquePermissions = permissions
            .GroupBy(p => p.Value)
            .Select(g => g.First())
            .OrderBy(p => p.Value)
            .ToList();

        // Calculate a deterministic hash to allow skipping sync if metadata hasn't changed.
        var metadataHash = CalculateDeterministicHash(uniquePermissions);

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using VK.Blocks.Authorization.Features.Permissions;");
        sb.AppendLine();
        sb.AppendLine("namespace VK.Blocks.Authorization.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Strongly-typed catalog of all permissions discovered via definition and usage.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static class PermissionsCatalog");
        sb.AppendLine("    {");

        foreach (var p in uniquePermissions)
        {
            var identifier = GetPrefixedIdentifier(p);
            sb.AppendLine($"        /// <summary>Value: <c>{p.Value}</c></summary>");
            sb.AppendLine($"        public const string {identifier} = \"{p.Value}\";");
        }

        sb.AppendLine();
        sb.AppendLine($"        /// <summary>Deterministic hash of all permission metadata (Names, Descriptions, Modules).</summary>");
        sb.AppendLine($"        public const string MetadataHash = \"{metadataHash}\";");
        sb.AppendLine();

        sb.AppendLine("        /// <summary>A complete list of all permissions defined in the system.</summary>");
        sb.AppendLine("        public static readonly IReadOnlyList<Permission> All = new List<Permission>");
        sb.AppendLine("        {");
        foreach (var p in uniquePermissions)
        {
            var identifier = GetPrefixedIdentifier(p);
            var desc = string.IsNullOrEmpty(p.DisplayName) || p.DisplayName == p.SuggestedIdentifier
                ? p.Description
                : $"{p.DisplayName} | {p.Description}";

            sb.AppendLine($"            new Permission {{ Name = {identifier}, Module = \"{p.Module}\", Description = \"{desc}\" }},");
        }
        sb.AppendLine("        }.AsReadOnly();");

        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    #region Typed Permission Attributes");
        sb.AppendLine();
        foreach (var p in uniquePermissions)
        {
            var identifier = GetPrefixedIdentifier(p);
            var attributeName = $"Require{identifier}Attribute";

            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Marks a controller or action as requiring the <c>{p.Value}</c> permission.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = true)]");
            sb.AppendLine($"    public sealed class {attributeName}() : VK.Blocks.Authorization.Features.Permissions.AuthorizePermissionAttribute(\"{p.Value}\");");
            sb.AppendLine();
        }
        sb.AppendLine("    #endregion");
        sb.AppendLine("}");

        context.AddSource("PermissionsCatalog.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    /// <summary>
    /// Calculates a stable FNV-1a 64-bit hash as a hex string.
    /// </summary>
    private static string CalculateDeterministicHash(IEnumerable<PermissionInfo> permissions)
    {
        unchecked
        {
            var hash = 14695981039346656037UL; // FNV offset basis
            foreach (var p in permissions)
            {
                hash = UpdateHash(hash, p.Value);
                hash = UpdateHash(hash, p.Module);
                hash = UpdateHash(hash, p.DisplayName ?? "");
                hash = UpdateHash(hash, p.Description ?? "");
            }
            return hash.ToString("X16");
        }
    }

    private static ulong UpdateHash(ulong hash, string value)
    {
        foreach (var c in value)
        {
            hash ^= (ulong)c;
            hash *= 1099511628211UL; // FNV prime
        }
        return hash;
    }

    private static string GetPrefixedIdentifier(PermissionInfo p)
    {
        var baseName = p.SuggestedIdentifier ?? GetSafeIdentifier(p.Value);

        // Ensure module prefix to avoid naming collisions across modules
        if (!string.IsNullOrEmpty(p.Module) && !baseName.StartsWith(p.Module, System.StringComparison.OrdinalIgnoreCase))
        {
            return p.Module + baseName;
        }

        return baseName;
    }

    private static string GetSafeIdentifier(string value)
    {
        var safeName = new string(value.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
        if (safeName.Length > 0 && char.IsLower(safeName[0]))
        {
            safeName = char.ToUpper(safeName[0]) + safeName.Substring(1);
        }

        if (safeName.Length > 0 && char.IsDigit(safeName[0]))
        {
            safeName = "_" + safeName;
        }

        return safeName;
    }

    private record PermissionInfo(string Value, string Module, string? SuggestedIdentifier, string? DisplayName = null, string? Description = null);
}
