using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.MultiTenancy.EFCore.Common.Diagnostics.Internal;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.MultiTenancy.EFCore;

/// <summary>
/// Interceptor for multi-tenancy concerns, including TenantId injection during saves
/// and Schema switching during connection opening.
/// </summary>
public sealed class VKTenantInterceptor(
    IVKTenantProvider tenantProvider,
    IVKTenantContext tenantContext,
    ILogger<VKTenantInterceptor> logger) : DbConnectionInterceptor, ISaveChangesInterceptor
{
    private readonly IVKTenantProvider _tenantProvider = VKGuard.NotNull(tenantProvider);
    private readonly IVKTenantContext _tenantContext = VKGuard.NotNull(tenantContext);
    private readonly ILogger<VKTenantInterceptor> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        InjectTenantId(eventData.Context);
        return result;
    }

    /// <inheritdoc />
    public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        InjectTenantId(eventData.Context);
        return ValueTask.FromResult(result);
    }

    /// <inheritdoc />
    public InterceptionResult<int> SavedChanges(SaveChangesCompletedEventData eventData, InterceptionResult<int> result) => result;

    /// <inheritdoc />
    public ValueTask<InterceptionResult<int>> SavedChangesAsync(SaveChangesCompletedEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) => ValueTask.FromResult(result);

    /// <inheritdoc />
    public InterceptionResult SaveChangesFailed(DbContextErrorEventData eventData) => default;

    /// <inheritdoc />
    public Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default) => Task.CompletedTask;

    private void InjectTenantId(DbContext? context)
    {
        if (context is null)
            return;

        if (context is VKBaseDbContext vkContext && !vkContext.IsMultiTenancyEnabled)
        {
            return;
        }

        var currentTenantId = _tenantProvider.GetCurrentTenantId();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            var type = entry.Entity.GetType();

            if (!VKEntityMetadata.IsMultiTenant(type) || entry.Entity is not IVKTenantScoped multiTenantEntity)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                if (currentTenantId.IsNullOrEmpty())
                {
                    // If context has no tenant, allow explicit TenantId assignment (e.g. system repository or admin creation)
                    if (!multiTenantEntity.TenantId.IsNullOrEmpty())
                    {
                        continue;
                    }

                    throw new InvalidOperationException($"Cannot save IVKTenantScoped entity of type '{type.Name}': TenantId is missing from context.");
                }

                if (multiTenantEntity.TenantId.IsNullOrEmpty())
                {
                    entry.Property(nameof(IVKTenantScoped.TenantId)).CurrentValue = currentTenantId;
                }
                else if (multiTenantEntity.TenantId != currentTenantId)
                {
                    throw new InvalidOperationException(
                        $"Cross-tenant security violation: Entity '{type.Name}' has TenantId '{multiTenantEntity.TenantId}', which does not match current context TenantId '{currentTenantId}'.");
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                var tenantProp = entry.Property(nameof(IVKTenantScoped.TenantId));
                if (tenantProp.IsModified && !Equals(tenantProp.OriginalValue, tenantProp.CurrentValue))
                {
                    throw new InvalidOperationException(
                        $"Immutable tenant violation: TenantId of entity '{type.Name}' cannot be modified after creation (Original: '{tenantProp.OriginalValue}', New: '{tenantProp.CurrentValue}').");
                }

                if (!currentTenantId.IsNullOrEmpty() && multiTenantEntity.TenantId != currentTenantId)
                {
                    throw new InvalidOperationException(
                        $"Cross-tenant security violation: Cannot modify entity '{type.Name}' belonging to TenantId '{multiTenantEntity.TenantId}' within current context TenantId '{currentTenantId}'.");
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                if (!currentTenantId.IsNullOrEmpty() && multiTenantEntity.TenantId != currentTenantId)
                {
                    throw new InvalidOperationException(
                        $"Cross-tenant security violation: Cannot delete entity '{type.Name}' belonging to TenantId '{multiTenantEntity.TenantId}' within current context TenantId '{currentTenantId}'.");
                }
            }
        }
    }

    /// <inheritdoc />
    public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
    {
        ApplySchemaIsolation(connection, eventData.Context);
        return base.ConnectionOpening(connection, eventData, result);
    }

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
    {
        await ApplySchemaIsolationAsync(connection, eventData.Context, cancellationToken).ConfigureAwait(false);
        return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private void ApplySchemaIsolation(DbConnection connection, DbContext? context)
    {
        var schema = _tenantContext.Schema;
        if (string.IsNullOrWhiteSpace(schema))
            return;

        using var command = connection.CreateCommand();
        command.CommandText = GetSchemaSwitchCommand(context, schema);

        if (string.IsNullOrEmpty(command.CommandText))
            return;

        _logger.LogSwitchingSchema(schema, _tenantContext.TenantId.Value.ToString());
        command.ExecuteNonQuery();
    }

    private async Task ApplySchemaIsolationAsync(DbConnection connection, DbContext? context, CancellationToken ct)
    {
        var schema = _tenantContext.Schema;
        if (string.IsNullOrWhiteSpace(schema))
            return;

        await using var command = connection.CreateCommand();
        command.CommandText = GetSchemaSwitchCommand(context, schema);

        if (string.IsNullOrEmpty(command.CommandText))
            return;

        _logger.LogSwitchingSchemaAsync(schema, _tenantContext.TenantId.Value.ToString());
        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static string GetSchemaSwitchCommand(DbContext? context, string schema)
    {
        if (context is null)
            return string.Empty;

        var provider = context.Database.ProviderName;

        if (provider?.Contains("Npgsql") == true)
        {
            return $"SET search_path TO {schema}";
        }

        return string.Empty;
    }
}
