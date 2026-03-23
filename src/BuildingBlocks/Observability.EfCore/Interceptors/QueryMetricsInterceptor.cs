using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VK.Blocks.Observability.EfCore.Diagnostics;

namespace VK.Blocks.Observability.EfCore.Interceptors;

internal sealed class QueryMetricsInterceptor : DbCommandInterceptor
{
    private static readonly Counter<long> _queryCounter;
    private static readonly Histogram<double> _queryDuration;

    static QueryMetricsInterceptor()
    {
        _queryCounter = EfCoreDiagnostics.Meter.CreateCounter<long>(
            "vk_blocks_efcore_queries_total",
            description: "Total number of EF Core queries executed");

        _queryDuration = EfCoreDiagnostics.Meter.CreateHistogram<double>(
            "vk_blocks_efcore_query_duration",
            unit: "ms",
            description: "Duration of EF Core queries");
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        DbDataReader result)
    {
        RecordMetrics(eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        DbDataReader result, 
        CancellationToken cancellationToken = default)
    {
        RecordMetrics(eventData);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        int result)
    {
        RecordMetrics(eventData);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        int result, 
        CancellationToken cancellationToken = default)
    {
        RecordMetrics(eventData);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        object? result)
    {
        RecordMetrics(eventData);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        object? result, 
        CancellationToken cancellationToken = default)
    {
        RecordMetrics(eventData);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override void CommandFailed(
        DbCommand command, 
        CommandErrorEventData eventData)
    {
        RecordMetrics(eventData, isError: true);
        base.CommandFailed(command, eventData);
    }

    public override Task CommandFailedAsync(
        DbCommand command, 
        CommandErrorEventData eventData, 
        CancellationToken cancellationToken = default)
    {
        RecordMetrics(eventData, isError: true);
        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    private static void RecordMetrics(CommandEndEventData eventData, bool isError = false)
    {
        var tags = new TagList 
        {
            { "db.system", "efcore" },
            { "error", isError }
        };
        
        _queryCounter.Add(1, tags);
        
        // Duration in CommandEndEventData is TimeSpan, we take TotalMilliseconds
        _queryDuration.Record(eventData.Duration.TotalMilliseconds, tags);
    }
}
