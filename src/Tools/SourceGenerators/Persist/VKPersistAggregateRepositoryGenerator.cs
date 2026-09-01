using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.Persist;

/// <summary>
/// Incremental Source Generator that emits strongly-typed Domain Aggregate Repositories
/// and their Dependency Injection extensions for persistence entities decorated with <c>[VKPersistEntity]</c>.
/// Produces:
/// 1. <c>internal sealed class {BaseName}Repository : IVK{BaseName}Repository</c> (Production-grade Aggregate Repository with automated OpenTelemetry Tracing)
/// 2. <c>public static IServiceCollection AddGeneratedAggregateRepositories(this IServiceCollection services)</c> (Automated DI hook)
/// Follows AP.01, AP.03, BB.01, BB.03, CS.01, CS.03, OR.01.
/// </summary>
[Generator]
public sealed class VKPersistAggregateRepositoryGenerator : IIncrementalGenerator
{
    private const string PersistEntityAttributeFullName = "VK.Blocks.Core.VKPersistEntityAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var entityDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0,
                transform: static (ctx, _) => TransformTargetWithSymbol(ctx))
            .Where(static t => t.Symbol is not null);

        var validRepos = entityDeclarations
            .Where(static t => t.Info is not null)
            .Select(static (t, _) => t.Info!);

        context.RegisterSourceOutput(validRepos, static (ctx, entity) => EmitAggregateRepository(ctx, entity));

        // Group by Assembly to emit DI registration extension
        var collected = entityDeclarations.Collect();
        context.RegisterSourceOutput(collected, static (ctx, list) => EmitDIExtensions(ctx, list));
    }

    private static (INamedTypeSymbol? Symbol, AggregateRepositoryInfo? Info) TransformTargetWithSymbol(GeneratorSyntaxContext ctx)
    {
        if (ctx.Node is not ClassDeclarationSyntax classSyntax)
            return (null, null);
        var symbol = ctx.SemanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;
        if (symbol is null)
            return (null, null);

        var attr = symbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == PersistEntityAttributeFullName);

        if (attr is null)
            return (null, null);

        var info = TransformTarget(symbol, attr, ctx.SemanticModel.Compilation);
        return (symbol, info);
    }

    private static AggregateRepositoryInfo? TransformTarget(INamedTypeSymbol entitySymbol, AttributeData attr, Compilation compilation)
    {
        // Must have DomainType provided
        if (attr.ConstructorArguments.Length < 1)
            return null;

        var domainSymbol = attr.ConstructorArguments[0].Value as INamedTypeSymbol;
        if (domainSymbol is null)
            return null;

        // Check GenerateAggregateRepository switch
        foreach (var namedArg in attr.NamedArguments)
        {
            if (string.Equals(namedArg.Key, "GenerateAggregateRepository", StringComparison.OrdinalIgnoreCase))
            {
                if (namedArg.Value.Value is bool b && !b)
                {
                    return null;
                }
            }
        }

        // Check if domainSymbol inherits from VKAggregateRoot<TId>
        bool isAggregateRoot = false;
        string? idTypeFullName = null;

        var currentBase = domainSymbol.BaseType;
        while (currentBase is not null)
        {
            if (currentBase.Name.StartsWith("VKAggregateRoot") ||
                currentBase.AllInterfaces.Any(i => i.Name.StartsWith("IVKAggregateRoot")))
            {
                isAggregateRoot = true;
                if (currentBase.TypeArguments.Length > 0)
                {
                    idTypeFullName = currentBase.TypeArguments[0].ToDisplayString();
                }
                break;
            }
            currentBase = currentBase.BaseType;
        }

        // If not an Aggregate Root, do NOT generate an Aggregate Repository
        if (!isAggregateRoot || idTypeFullName is null)
        {
            return null;
        }

        // Read ProjectBy parameter (collection of navigation property names to Include by default)
        var projectBy = new List<string>();
        var projectByArg = attr.NamedArguments.FirstOrDefault(a => a.Key == "ProjectBy");
        if (projectByArg.Value.Kind == TypedConstantKind.Array && !projectByArg.Value.Values.IsDefaultOrEmpty)
        {
            foreach (var val in projectByArg.Value.Values)
            {
                if (val.Value is string s && !string.IsNullOrWhiteSpace(s))
                {
                    projectBy.Add(s);
                }
            }
        }

        var entityName = entitySymbol.Name;
        // Compute base domain name (e.g. VKPsycheKnowledgeEntity -> PsycheKnowledge, VKUserEntity -> User)
        var baseName = entityName;
        if (baseName.StartsWith("VK"))
        {
            baseName = baseName.Substring(2);
        }
        if (baseName.EndsWith("Entity"))
        {
            baseName = baseName.Substring(0, baseName.Length - "Entity".Length);
        }

        // Check for Custom Interface override
        string interfaceName;
        string interfaceFullName;
        string implementationName = $"{baseName}Repository";

        var customInterfaceArg = attr.NamedArguments.FirstOrDefault(a => a.Key == "RepositoryInterfaceType");
        if (customInterfaceArg.Value.Value is INamedTypeSymbol customInterfaceSymbol)
        {
            interfaceName = customInterfaceSymbol.Name;
            interfaceFullName = customInterfaceSymbol.ToDisplayString();
        }
        else
        {
            // Default convention: IVK{baseName}Repository (e.g. IVKPsycheKnowledgeRepository)
            interfaceName = $"IVK{baseName}Repository";
            var domainNamespace = domainSymbol.ContainingNamespace.ToDisplayString();
            interfaceFullName = $"{domainNamespace}.{interfaceName}";
        }

        return new AggregateRepositoryInfo(
            Namespace: entitySymbol.ContainingNamespace.ToDisplayString(),
            EntityFullName: entitySymbol.ToDisplayString(),
            EntityName: entityName,
            DomainFullName: domainSymbol.ToDisplayString(),
            DomainName: domainSymbol.Name,
            IdTypeFullName: idTypeFullName,
            InterfaceName: interfaceName,
            InterfaceFullName: interfaceFullName,
            ImplementationName: implementationName,
            ProjectBy: projectBy.ToImmutableArray()
        );
    }

    private static void EmitAggregateRepository(SourceProductionContext ctx, AggregateRepositoryInfo info)
    {
        var rawDomain = info.DomainName.StartsWith("VK") ? info.DomainName.Substring(2) : info.DomainName;
        var entitySnake = IdentifierUtilities.ToSnakeCase(rawDomain);
        var moduleName = IdentifierUtilities.ExtractModuleName(info.Namespace);
        var traceName = $"{moduleName}.repository.{entitySnake}";

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Diagnostics;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine("using VK.Blocks.Persistence;");
        sb.AppendLine();
        sb.AppendLine($"namespace {info.Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// EF Core aggregate repository implementation for <see cref=\"{info.DomainFullName}\"/>.");
        sb.AppendLine("/// Adapts persistence entity operations using the Source-Generated mapper.");
        sb.AppendLine("/// Follows AP.01 (sealed class default), CS.03 (ConfigureAwait(false)), and OR.01 (Distributed Tracing).");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        sb.AppendLine($"[VKTrace(\"{traceName}\")]");
        sb.AppendLine($"internal sealed partial class {info.ImplementationName}(");
        sb.AppendLine($"    IVKEntityRepository<{info.EntityFullName}> repository,");
        sb.AppendLine("    IVKUnitOfWork unitOfWork,");
        sb.AppendLine($"    ILogger<{info.ImplementationName}> logger) : {info.InterfaceFullName}");
        sb.AppendLine("{");
        sb.AppendLine($"    private readonly IVKEntityRepository<{info.EntityFullName}> _repository = VKGuard.NotNull(repository);");
        sb.AppendLine("    private readonly IVKUnitOfWork _unitOfWork = VKGuard.NotNull(unitOfWork);");
        sb.AppendLine($"    private readonly ILogger<{info.ImplementationName}> _logger = VKGuard.NotNull(logger);");
        sb.AppendLine();

        // ProjectBy include builder
        string includeClause = string.Empty;
        if (info.ProjectBy.Length > 0)
        {
            var includes = string.Join(".", info.ProjectBy.Select(p => $"Include(e => e.{p})"));
            includeClause = $", include: q => q.{includes}";
        }

        // FindByIdAsync
        sb.AppendLine($"    public async Task<VKResult<{info.DomainFullName}>> FindByIdAsync({info.IdTypeFullName} id, CancellationToken ct = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (id.IsEmpty)");
        sb.AppendLine("        {");
        sb.AppendLine($"            return VKResult.Failure<{info.DomainFullName}>(VKPersistenceErrors.Repository.EntityNotFound);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity(\"db.{entitySnake}.find_by_id\") : null;");
        sb.AppendLine("        activity?.SetTag(\"db.system\", \"efcore\");");
        sb.AppendLine($"        activity?.SetTag(\"db.entity\", \"{info.DomainName}\");");
        sb.AppendLine("        activity?.SetTag(\"db.operation\", \"FindById\");");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine($"            var entity = await _repository.GetFirstOrDefaultAsync(predicate: d => d.Id == id{includeClause}, cancellationToken: ct).ConfigureAwait(false);");
        sb.AppendLine("            if (entity is null)");
        sb.AppendLine("            {");
        sb.AppendLine($"                activity?.SetStatus(ActivityStatusCode.Error, \"Entity not found\");");
        sb.AppendLine($"                return VKResult.Failure<{info.DomainFullName}>(VKPersistenceErrors.Repository.EntityNotFound);");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            activity?.SetStatus(ActivityStatusCode.Ok);");
        sb.AppendLine("            return VKResult.Success(entity.ToDomain());");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine("            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);");
        sb.AppendLine($"            _logger.LogError(ex, \"Failed to find aggregate {info.DomainName} with ID: {{Id}}\", id);");
        sb.AppendLine($"            return VKResult.Failure<{info.DomainFullName}>(VKPersistenceErrors.Database.ExecutionFailed);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        // ListByIdsAsync
        sb.AppendLine($"    public async Task<VKResult<IReadOnlyList<{info.DomainFullName}>>> ListByIdsAsync(");
        sb.AppendLine($"        IReadOnlyList<{info.IdTypeFullName}> ids,");
        sb.AppendLine("        CancellationToken ct = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        ct.ThrowIfCancellationRequested();");
        sb.AppendLine("        VKGuard.NotNull(ids);");
        sb.AppendLine();
        sb.AppendLine("        if (ids.Count == 0)");
        sb.AppendLine("        {");
        sb.AppendLine($"            return VKResult.Success<IReadOnlyList<{info.DomainFullName}>>([]);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity(\"db.{entitySnake}.list_by_ids\") : null;");
        sb.AppendLine("        activity?.SetTag(\"db.system\", \"efcore\");");
        sb.AppendLine($"        activity?.SetTag(\"db.entity\", \"{info.DomainName}\");");
        sb.AppendLine("        activity?.SetTag(\"db.operation\", \"ListByIds\");");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine($"            var entities = await _repository.GetListAsync(predicate: d => ids.Contains(d.Id){includeClause}, cancellationToken: ct).ConfigureAwait(false);");
        sb.AppendLine("            var domainList = entities.Select(e => e.ToDomain()).ToList().AsReadOnly();");
        sb.AppendLine("            activity?.SetTag(\"db.result.count\", domainList.Count);");
        sb.AppendLine("            activity?.SetStatus(ActivityStatusCode.Ok);");
        sb.AppendLine($"            return VKResult.Success<IReadOnlyList<{info.DomainFullName}>>(domainList);");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine("            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);");
        sb.AppendLine($"            _logger.LogError(ex, \"Failed to list aggregates {info.DomainName} for IDs: {{Ids}}\", string.Join(\",\", ids));");
        sb.AppendLine($"            return VKResult.Failure<IReadOnlyList<{info.DomainFullName}>>(VKPersistenceErrors.Database.ExecutionFailed);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        // ListAllAsync
        sb.AppendLine($"    public async Task<VKResult<IReadOnlyList<{info.DomainFullName}>>> ListAllAsync(CancellationToken ct = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        ct.ThrowIfCancellationRequested();");
        sb.AppendLine();
        sb.AppendLine($"        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity(\"db.{entitySnake}.list_all\") : null;");
        sb.AppendLine("        activity?.SetTag(\"db.system\", \"efcore\");");
        sb.AppendLine($"        activity?.SetTag(\"db.entity\", \"{info.DomainName}\");");
        sb.AppendLine("        activity?.SetTag(\"db.operation\", \"ListAll\");");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine($"            var entities = await _repository.GetListAsync(predicate: d => true{includeClause}, cancellationToken: ct).ConfigureAwait(false);");
        sb.AppendLine("            var domainList = entities.Select(e => e.ToDomain()).ToList().AsReadOnly();");
        sb.AppendLine("            activity?.SetTag(\"db.result.count\", domainList.Count);");
        sb.AppendLine("            activity?.SetStatus(ActivityStatusCode.Ok);");
        sb.AppendLine($"            return VKResult.Success<IReadOnlyList<{info.DomainFullName}>>(domainList);");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine("            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);");
        sb.AppendLine($"            _logger.LogError(ex, \"Failed to list all aggregates of {info.DomainName}\");");
        sb.AppendLine($"            return VKResult.Failure<IReadOnlyList<{info.DomainFullName}>>(VKPersistenceErrors.Database.ExecutionFailed);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        // ExistsAsync
        sb.AppendLine($"    public async Task<bool> ExistsAsync({info.IdTypeFullName} id, CancellationToken ct = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (id.IsEmpty)");
        sb.AppendLine("        {");
        sb.AppendLine("            return false;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        return await _repository.AnyAsync(d => d.Id == id, cancellationToken: ct).ConfigureAwait(false);");
        sb.AppendLine("    }");
        sb.AppendLine();

        // AddAsync
        sb.AppendLine($"    public async Task<VKResult> AddAsync({info.DomainFullName} item, CancellationToken ct = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        VKGuard.NotNull(item);");
        sb.AppendLine();
        sb.AppendLine($"        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity(\"db.{entitySnake}.add\") : null;");
        sb.AppendLine("        activity?.SetTag(\"db.system\", \"efcore\");");
        sb.AppendLine($"        activity?.SetTag(\"db.entity\", \"{info.DomainName}\");");
        sb.AppendLine("        activity?.SetTag(\"db.operation\", \"Add\");");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine("            var entity = item.ToEntity();");
        sb.AppendLine("            await _repository.AddAsync(entity, ct).ConfigureAwait(false);");
        sb.AppendLine("            await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);");
        sb.AppendLine("            activity?.SetStatus(ActivityStatusCode.Ok);");
        sb.AppendLine("            return VKResult.Success();");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine("            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);");
        sb.AppendLine($"            _logger.LogError(ex, \"Failed to add aggregate {info.DomainName} with ID: {{Id}}\", item.Id);");
        sb.AppendLine("            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        // UpdateAsync
        sb.AppendLine($"    public async Task<VKResult> UpdateAsync({info.DomainFullName} item, CancellationToken ct = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        VKGuard.NotNull(item);");
        sb.AppendLine();
        sb.AppendLine($"        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity(\"db.{entitySnake}.update\") : null;");
        sb.AppendLine("        activity?.SetTag(\"db.system\", \"efcore\");");
        sb.AppendLine($"        activity?.SetTag(\"db.entity\", \"{info.DomainName}\");");
        sb.AppendLine("        activity?.SetTag(\"db.operation\", \"Update\");");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");

        if (info.ProjectBy.Length > 0)
        {
            var includes = string.Join(".", info.ProjectBy.Select(p => $"Include(e => e.{p})"));
            sb.AppendLine("            var trackResult = await _repository.TrackAndUpdateAsync(");
            sb.AppendLine("                predicate: d => d.Id == item.Id,");
            sb.AppendLine("                domain: item,");
            sb.AppendLine("                mapOntoAction: static (domain, entity) => domain.MapOnto(entity),");
            sb.AppendLine("                notFoundError: VKPersistenceErrors.Repository.EntityNotFound,");
            sb.AppendLine($"                include: q => q.{includes},");
            sb.AppendLine("                ct: ct).ConfigureAwait(false);");
        }
        else
        {
            sb.AppendLine("            var trackResult = await _repository.TrackAndUpdateByIdAsync(");
            sb.AppendLine("                id: item.Id,");
            sb.AppendLine("                domain: item,");
            sb.AppendLine("                mapOntoAction: static (domain, entity) => domain.MapOnto(entity),");
            sb.AppendLine("                notFoundError: VKPersistenceErrors.Repository.EntityNotFound,");
            sb.AppendLine("                ct: ct).ConfigureAwait(false);");
        }

        sb.AppendLine();
        sb.AppendLine("            if (trackResult.IsFailure)");
        sb.AppendLine("            {");
        sb.AppendLine("                activity?.SetStatus(ActivityStatusCode.Error, trackResult.FirstError.Description);");
        sb.AppendLine("                return trackResult;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);");
        sb.AppendLine("            activity?.SetStatus(ActivityStatusCode.Ok);");
        sb.AppendLine("            return VKResult.Success();");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine("            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);");
        sb.AppendLine($"            _logger.LogError(ex, \"Failed to update aggregate {info.DomainName} with ID: {{Id}}\", item.Id);");
        sb.AppendLine("            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        // DeleteAsync
        sb.AppendLine($"    public async Task<VKResult> DeleteAsync({info.IdTypeFullName} id, CancellationToken ct = default)");
        sb.AppendLine("    {");
        sb.AppendLine($"        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity(\"db.{entitySnake}.delete\") : null;");
        sb.AppendLine("        activity?.SetTag(\"db.system\", \"efcore\");");
        sb.AppendLine($"        activity?.SetTag(\"db.entity\", \"{info.DomainName}\");");
        sb.AppendLine("        activity?.SetTag(\"db.operation\", \"Delete\");");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine($"            var entity = await _repository.GetTrackedFirstOrDefaultAsync(predicate: d => d.Id == id{includeClause}, cancellationToken: ct).ConfigureAwait(false);");
        sb.AppendLine("            if (entity is null)");
        sb.AppendLine("            {");
        sb.AppendLine("                activity?.SetStatus(ActivityStatusCode.Error, \"Entity not found\");");
        sb.AppendLine("                return VKResult.Failure(VKPersistenceErrors.Repository.EntityNotFound);");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            await _repository.DeleteAsync(entity, ct).ConfigureAwait(false);");
        sb.AppendLine("            await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);");
        sb.AppendLine("            activity?.SetStatus(ActivityStatusCode.Ok);");
        sb.AppendLine("            return VKResult.Success();");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine("            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);");
        sb.AppendLine($"            _logger.LogError(ex, \"Failed to delete aggregate {info.DomainName} with ID: {{Id}}\", id);");
        sb.AppendLine("            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource($"{info.ImplementationName}.g.cs", sb.ToString());
    }

    private static void EmitDIExtensions(SourceProductionContext ctx, ImmutableArray<(INamedTypeSymbol? Symbol, AggregateRepositoryInfo? Info)> list)
    {
        var validSymbols = list.Where(x => x.Symbol is not null).ToList();
        if (validSymbols.Count == 0)
            return;

        var first = validSymbols.First().Symbol!;
        var ns = first.ContainingNamespace.ToDisplayString();
        var validRepos = validSymbols.Where(x => x.Info is not null).Select(x => x.Info!).Distinct().ToList();

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {ns};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Automated Dependency Injection registrations for Source-Generated Aggregate Repositories.");
        sb.AppendLine("/// Follows AP.02 (TryAdd only) and BB.03.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        sb.AppendLine("public static class GeneratedAggregateRepositoriesExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Registers all aggregate repositories generated from <c>[VKPersistEntity]</c> in this assembly.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IServiceCollection AddGeneratedAggregateRepositories(this IServiceCollection services)");
        sb.AppendLine("    {");
        sb.AppendLine("        VKGuard.NotNull(services);");
        sb.AppendLine();

        foreach (var info in validRepos)
        {
            sb.AppendLine($"        services.AddScoped<{info.InterfaceFullName}, {info.ImplementationName}>();");
        }

        sb.AppendLine();
        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource("GeneratedAggregateRepositoriesExtensions.g.cs", sb.ToString());
    }

    private sealed record AggregateRepositoryInfo(
        string Namespace,
        string EntityFullName,
        string EntityName,
        string DomainFullName,
        string DomainName,
        string IdTypeFullName,
        string InterfaceName,
        string InterfaceFullName,
        string ImplementationName,
        ImmutableArray<string> ProjectBy
    );
}
