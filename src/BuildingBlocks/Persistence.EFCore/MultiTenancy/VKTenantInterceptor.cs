using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.MultiTenancy;
using VK.Blocks.Persistence.EFCore.Diagnostics.Internal;

namespace VK.Blocks.Persistence.EFCore;

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

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added)
                continue;

            var type = entry.Entity.GetType();

            if (VKEntityMetadata.IsMultiTenantEntity(type) && entry.Entity is IVKMultiTenantEntity multiTenantEntity)
            {
                if (string.IsNullOrWhiteSpace(multiTenantEntity.TenantId))
                {
                    var tenantId = _tenantProvider.GetCurrentTenantId();
                    if (string.IsNullOrWhiteSpace(tenantId))
                    {
                        throw new InvalidOperationException($"Cannot save IVKMultiTenant entity of type '{type.Name}': TenantId is missing from context.");
                    }
                    multiTenantEntity.TenantId = tenantId;
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
        var schema = _tenantContext.CurrentTenant?.Schema;
        if (string.IsNullOrWhiteSpace(schema))
            return;

        using var command = connection.CreateCommand();
        command.CommandText = GetSchemaSwitchCommand(context, schema);

        if (string.IsNullOrEmpty(command.CommandText))
            return;

        _logger.LogSwitchingSchema(schema, _tenantContext.CurrentTenant!.Id);
        command.ExecuteNonQuery();
    }

    private async Task ApplySchemaIsolationAsync(DbConnection connection, DbContext? context, CancellationToken ct)
    {
        var schema = _tenantContext.CurrentTenant?.Schema;
        if (string.IsNullOrWhiteSpace(schema))
            return;

        await using var command = connection.CreateCommand();
        command.CommandText = GetSchemaSwitchCommand(context, schema);

        if (string.IsNullOrEmpty(command.CommandText))
            return;

        _logger.LogSwitchingSchemaAsync(schema, _tenantContext.CurrentTenant!.Id);
        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static string GetSchemaSwitchCommand(DbContext? context, string schema)
    {
        if (context is null)
            return string.Empty;

        var provider = context.Database.ProviderName;

        // PostgreSQL: SET search_path TO schema
        if (provider?.Contains("Npgsql") == true)
        {
            return $"SET search_path TO {schema}";
        }

        // SQL Server: Not natively supported via connection-level switch in a clean way 
        // without affecting permissions, usually handled via HasDefaultSchema in OnModelCreating.
        // But for dynamic switches, some use custom SESSION_CONTEXT or equivalent.

        return string.Empty;
    }


}
