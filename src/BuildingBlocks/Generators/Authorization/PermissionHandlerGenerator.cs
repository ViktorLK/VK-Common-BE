using System;
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
/// Generates IPermissionProvider implementations for classes/enums marked with [GeneratePermissionHandler].
/// </summary>
[Generator]
public sealed class PermissionHandlerGenerator : IIncrementalGenerator
{
    private const string HandlerAttributeName = "GeneratePermissionHandler";
    private const string PermissionAttributeName = "GeneratePermissions";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var providerInfos = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is TypeDeclarationSyntax { AttributeLists.Count: > 0 } or EnumDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: GetProviderInfo)
            .WhereNotNull();

        var combined = providerInfos.Collect().Combine(assemblyName);
        context.RegisterSourceOutput(combined, (spc, pair) => Execute(spc, pair.Left, pair.Right, this.GetType()));
    }

    private static ProviderInfo? GetProviderInfo(GeneratorSyntaxContext context, System.Threading.CancellationToken ct)
    {
        var node = context.Node;
        ISymbol? symbol = node switch
        {
            TypeDeclarationSyntax tds => context.SemanticModel.GetDeclaredSymbol(tds, ct),
            EnumDeclarationSyntax eds => context.SemanticModel.GetDeclaredSymbol(eds, ct),
            _ => null
        };

        if (symbol is not INamedTypeSymbol typeSymbol)
            return null;

        var attributes = typeSymbol.GetAttributes();
        var handlerAttr = attributes.FirstOrDefault(a => a.AttributeClass?.Name.Contains(HandlerAttributeName) == true);
        var permAttr = attributes.FirstOrDefault(a => a.AttributeClass?.Name.Contains(PermissionAttributeName) == true);

        if (handlerAttr is null || permAttr is null)
            return null;

        var source = "Claims";
        string? moduleName = null;

        foreach (var namedArg in handlerAttr.NamedArguments)
        {
            if (namedArg.Key == "Source")
            {
                // PermissionSource enum
                // 0: Claims, 1: Database
                source = namedArg.Value.Value switch
                {
                    1 => "Database",
                    _ => "Claims"
                };
            }
            else if (namedArg.Key == "ModuleName")
            {
                moduleName = namedArg.Value.Value?.ToString();
            }
        }

        // Infer module name if not specified
        moduleName ??= typeSymbol.Name.Replace("Permissions", "").Replace("Catalog", "");

        return new ProviderInfo(
            moduleName,
            source,
            typeSymbol.ToDisplayString());
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<ProviderInfo> providers, string? assemblyName, Type generatorType)
    {
        // Guard against execution in unrelated assemblies
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
        {
            return;
        }

        if (providers.IsDefaultOrEmpty)
            return;

        foreach (var info in providers.Distinct())
        {
            GenerateCode(context, info);
        }
    }

    private static void GenerateCode(SourceProductionContext context, ProviderInfo info)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System.Security.Claims;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine("using VK.Blocks.Authorization.DependencyInjection;");
        sb.AppendLine("using VK.Blocks.Authorization.Features.Permissions;");
        sb.AppendLine("using VK.Blocks.Authorization.Features.Permissions.Persistence;");
        sb.AppendLine("using VK.Blocks.Core.Results;");
        sb.AppendLine();
        sb.AppendLine("namespace VK.Blocks.Authorization.Generated");
        sb.AppendLine("{");

        var className = $"{info.ModuleName}PermissionProvider";
        var isDatabase = info.Source == "Database";

        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// {info.Source}-backed permission provider for the {info.ModuleName} module.");
        sb.AppendLine("    /// </summary>");

        if (isDatabase)
        {
            sb.AppendLine($"    public sealed class {className}(IPermissionStore store) : IPermissionProvider");
        }
        else
        {
            sb.AppendLine($"    public sealed class {className}(IOptions<VKAuthorizationOptions> options) : IPermissionProvider");
        }

        sb.AppendLine("    {");

        if (isDatabase)
        {
            sb.AppendLine("        private readonly IPermissionStore _store = store;");
        }
        else
        {
            sb.AppendLine("        private readonly VKAuthorizationOptions _options = options.Value;");
        }

        sb.AppendLine();
        sb.AppendLine("        /// <inheritdoc />");
        sb.AppendLine("        public async ValueTask<Result<bool>> HasPermissionAsync(ClaimsPrincipal user, string permission, CancellationToken ct = default)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (user.Identity?.IsAuthenticated != true)");
        sb.AppendLine("            {");
        sb.AppendLine("                return Result.Success(false);");
        sb.AppendLine("            }");
        sb.AppendLine();

        if (isDatabase)
        {
            sb.AppendLine("            // Database check via IPermissionStore");
            sb.AppendLine("            return await _store.HasPermissionAsync(user, permission, ct).ConfigureAwait(false);");
        }
        else
        {
            sb.AppendLine("            // Claims check using global configuration");
            sb.AppendLine("            var hasPermission = user.HasClaim(_options.PermissionClaimType, permission);");
            sb.AppendLine("            return Result.Success(hasPermission);");
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{className}.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
