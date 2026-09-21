using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.Identity.EFCore.TenantUser.Internal;

/// <summary>
/// EFCore repository implementation for <see cref="IVKIdentityTenantUserRepository"/>.
/// Adapts persistence entity operations to the domain model using Source-Generated <see cref="VKIdentityTenantUserMapper"/>.
/// Follows AP.01 (sealed class default), CS.01 (Result<T>), CS.03 (ConfigureAwait(false)), and OR.01.
/// </summary>
[VKTrace("identity.repository.tenant_user")]
internal sealed class IdentityTenantUserRepository(
    IVKEntityRepository<VKIdentityTenantUserEntity> repository,
    IVKUnitOfWork unitOfWork,
    ILogger<IdentityTenantUserRepository> logger) : IVKIdentityTenantUserRepository
{
    private readonly IVKEntityRepository<VKIdentityTenantUserEntity> _repository = VKGuard.NotNull(repository);
    private readonly IVKUnitOfWork _unitOfWork = VKGuard.NotNull(unitOfWork);
    private readonly ILogger<IdentityTenantUserRepository> _logger = VKGuard.NotNull(logger);

    public async Task<VKResult<VKTenantUser>> FindAsync(VKTenantId tenantId, VKUserId userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity("db.tenant_user.find") : null;
        activity?.SetTag("db.system", "efcore");
        activity?.SetTag("db.entity", "VKTenantUser");
        activity?.SetTag("db.operation", "Find");

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var relation = await _repository.GetFirstOrDefaultAsync(
                tu => tu.TenantId == tenantId && tu.UserId == userId,
                cancellationToken: ct).ConfigureAwait(false);

            stopwatch.Stop();
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "FindAsync", true);

            if (relation is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "TenantUser relation not found");
                return VKResult.Failure<VKTenantUser>(VKTenantUserErrors.NotFound);
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            return VKResult.Success(relation.ToDomain());
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "FindAsync", false);
            TenantUserDiagnostics.RecordTenantUserError("FindAsync");
            _logger.LogFindTenantUserError(ex, tenantId.ToString(), userId.ToString());
            return VKResult.Failure<VKTenantUser>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IReadOnlyList<VKTenantUser>>> ListByUserAsync(VKUserId userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity("db.tenant_user.list_by_user") : null;
        activity?.SetTag("db.system", "efcore");
        activity?.SetTag("db.entity", "VKTenantUser");
        activity?.SetTag("db.operation", "ListByUser");

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var list = await _repository.GetListAsync(
                tu => tu.UserId == userId,
                cancellationToken: ct).ConfigureAwait(false);

            var domainList = list.Select(e => e.ToDomain()).ToList().AsReadOnly();
            stopwatch.Stop();
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "ListByUserAsync", true);
            activity?.SetTag("db.result.count", domainList.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return VKResult.Success<IReadOnlyList<VKTenantUser>>(domainList);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "ListByUserAsync", false);
            TenantUserDiagnostics.RecordTenantUserError("ListByUserAsync");
            _logger.LogListByUserError(ex, userId.ToString());
            return VKResult.Failure<IReadOnlyList<VKTenantUser>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IReadOnlyList<VKTenantUser>>> ListByTenantAsync(VKTenantId tenantId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity("db.tenant_user.list_by_tenant") : null;
        activity?.SetTag("db.system", "efcore");
        activity?.SetTag("db.entity", "VKTenantUser");
        activity?.SetTag("db.operation", "ListByTenant");

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var list = await _repository.GetListAsync(
                tu => tu.TenantId == tenantId,
                cancellationToken: ct).ConfigureAwait(false);

            var domainList = list.Select(e => e.ToDomain()).ToList().AsReadOnly();
            stopwatch.Stop();
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "ListByTenantAsync", true);
            activity?.SetTag("db.result.count", domainList.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return VKResult.Success<IReadOnlyList<VKTenantUser>>(domainList);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "ListByTenantAsync", false);
            TenantUserDiagnostics.RecordTenantUserError("ListByTenantAsync");
            _logger.LogListByTenantError(ex, tenantId.ToString());
            return VKResult.Failure<IReadOnlyList<VKTenantUser>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<int>> CountByTenantAsync(VKTenantId tenantId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity("db.tenant_user.count_by_tenant") : null;
        activity?.SetTag("db.system", "efcore");
        activity?.SetTag("db.entity", "VKTenantUser");
        activity?.SetTag("db.operation", "CountByTenant");

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var count = await _repository.CountAsync(
                tu => tu.TenantId == tenantId,
                cancellationToken: ct).ConfigureAwait(false);

            stopwatch.Stop();
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "CountByTenantAsync", true);
            activity?.SetTag("db.result.count", count);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return VKResult.Success(count);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "CountByTenantAsync", false);
            TenantUserDiagnostics.RecordTenantUserError("CountByTenantAsync");
            _logger.LogCountByTenantError(ex, tenantId.ToString());
            return VKResult.Failure<int>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<bool> ExistsAsync(VKTenantId tenantId, VKUserId userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            return await _repository.AnyAsync(
                tu => tu.TenantId == tenantId && tu.UserId == userId,
                cancellationToken: ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            TenantUserDiagnostics.RecordTenantUserError("ExistsAsync");
            _logger.LogExistsError(ex, tenantId.ToString(), userId.ToString());
            return false;
        }
    }

    public async Task<VKResult> AddAsync(VKTenantUser tenantUser, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        VKGuard.NotNull(tenantUser);

        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity("db.tenant_user.add") : null;
        activity?.SetTag("db.system", "efcore");
        activity?.SetTag("db.entity", "VKTenantUser");
        activity?.SetTag("db.operation", "Add");

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var entity = tenantUser.ToEntity();
            await _repository.AddAsync(entity, ct).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

            stopwatch.Stop();
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "AddAsync", true);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "AddAsync", false);
            TenantUserDiagnostics.RecordTenantUserError("AddAsync");
            _logger.LogAddTenantUserError(ex, tenantUser.TenantId.ToString(), tenantUser.UserId.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> UpdateAsync(VKTenantUser tenantUser, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        VKGuard.NotNull(tenantUser);

        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity("db.tenant_user.update") : null;
        activity?.SetTag("db.system", "efcore");
        activity?.SetTag("db.entity", "VKTenantUser");
        activity?.SetTag("db.operation", "Update");

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var trackResult = await _repository.TrackAndUpdateAsync(
                predicate: tu => tu.TenantId == tenantUser.TenantId && tu.UserId == tenantUser.UserId,
                domain: tenantUser,
                mapOntoAction: static (domain, entity) => domain.MapOnto(entity),
                notFoundError: VKTenantUserErrors.NotFound,
                ct: ct).ConfigureAwait(false);

            if (trackResult.IsFailure)
            {
                stopwatch.Stop();
                activity?.SetStatus(ActivityStatusCode.Error, trackResult.FirstError.Description);
                TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "UpdateAsync", false);
                return trackResult;
            }

            await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            stopwatch.Stop();
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "UpdateAsync", true);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "UpdateAsync", false);
            TenantUserDiagnostics.RecordTenantUserError("UpdateAsync");
            _logger.LogUpdateTenantUserError(ex, tenantUser.TenantId.ToString(), tenantUser.UserId.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> RemoveAsync(VKTenantId tenantId, VKUserId userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity("db.tenant_user.remove") : null;
        activity?.SetTag("db.system", "efcore");
        activity?.SetTag("db.entity", "VKTenantUser");
        activity?.SetTag("db.operation", "Remove");

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var relation = await _repository.GetTrackedFirstOrDefaultAsync(
                tu => tu.TenantId == tenantId && tu.UserId == userId,
                cancellationToken: ct).ConfigureAwait(false);

            if (relation is null)
            {
                stopwatch.Stop();
                activity?.SetStatus(ActivityStatusCode.Error, "TenantUser relation not found");
                TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "RemoveAsync", false);
                return VKResult.Failure(VKTenantUserErrors.NotFound);
            }

            await _repository.DeleteAsync(relation, ct).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

            stopwatch.Stop();
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "RemoveAsync", true);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            TenantUserDiagnostics.RecordTenantUserOperation(stopwatch.Elapsed.TotalMilliseconds, "RemoveAsync", false);
            TenantUserDiagnostics.RecordTenantUserError("RemoveAsync");
            _logger.LogRemoveTenantUserError(ex, tenantId.ToString(), userId.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
