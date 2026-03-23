using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VK.Blocks.Observability.EfCore.Diagnostics;

namespace VK.Blocks.Observability.EfCore.Interceptors;

internal sealed class QueryTracingInterceptor : DbCommandInterceptor
{
    private readonly ConcurrentDictionary<Guid, Activity?> _activities = new();

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<DbDataReader> result)
    {
        StartActivity(eventData.CommandId, command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<DbDataReader> result, 
        CancellationToken cancellationToken = default)
    {
        StartActivity(eventData.CommandId, command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<int> result)
    {
        StartActivity(eventData.CommandId, command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        StartActivity(eventData.CommandId, command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object?> ScalarExecuting(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<object?> result)
    {
        StartActivity(eventData.CommandId, command);
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object?>> ScalarExecutingAsync(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<object?> result, 
        CancellationToken cancellationToken = default)
    {
        StartActivity(eventData.CommandId, command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        DbDataReader result)
    {
        StopActivity(eventData.CommandId);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        DbDataReader result, 
        CancellationToken cancellationToken = default)
    {
        StopActivity(eventData.CommandId);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        int result)
    {
        StopActivity(eventData.CommandId);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        int result, 
        CancellationToken cancellationToken = default)
    {
        StopActivity(eventData.CommandId);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        object? result)
    {
        StopActivity(eventData.CommandId);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        object? result, 
        CancellationToken cancellationToken = default)
    {
        StopActivity(eventData.CommandId);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override void CommandFailed(
        DbCommand command, 
        CommandErrorEventData eventData)
    {
        StopActivityWithError(eventData.CommandId, eventData.Exception);
        base.CommandFailed(command, eventData);
    }

    public override Task CommandFailedAsync(
        DbCommand command, 
        CommandErrorEventData eventData, 
        CancellationToken cancellationToken = default)
    {
        StopActivityWithError(eventData.CommandId, eventData.Exception);
        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    private void StartActivity(Guid commandId, DbCommand command)
    {
        var activity = EfCoreDiagnostics.Source.StartActivity("db.query");
        if (activity != null)
        {
            activity.SetTag("db.system", "efcore");
            activity.SetTag("db.statement", command.CommandText);
        }

        _activities.TryAdd(commandId, activity);
    }

    private void StopActivity(Guid commandId)
    {
        if (_activities.TryRemove(commandId, out var activity))
        {
            activity?.Dispose();
        }
    }

    private void StopActivityWithError(Guid commandId, Exception exception)
    {
        if (_activities.TryRemove(commandId, out var activity) && activity != null)
        {
            activity.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity.Dispose();
        }
    }
}
