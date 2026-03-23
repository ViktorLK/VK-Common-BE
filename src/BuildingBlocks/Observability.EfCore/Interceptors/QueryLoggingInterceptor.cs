using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Observability.EfCore.Options;

namespace VK.Blocks.Observability.EfCore.Interceptors;

internal sealed partial class QueryLoggingInterceptor : DbCommandInterceptor
{
    private readonly ILogger<QueryLoggingInterceptor> _logger;
    private readonly EfCoreObservabilityOptions _options;

    public QueryLoggingInterceptor(
        ILogger<QueryLoggingInterceptor> logger,
        IOptions<EfCoreObservabilityOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<DbDataReader> result)
    {
        LogCommand(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<DbDataReader> result, 
        CancellationToken cancellationToken = default)
    {
        LogCommand(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<int> result)
    {
        LogCommand(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        LogCommand(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object?> ScalarExecuting(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<object?> result)
    {
        LogCommand(command);
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object?>> ScalarExecutingAsync(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<object?> result, 
        CancellationToken cancellationToken = default)
    {
        LogCommand(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        DbDataReader result)
    {
        CheckSlowQuery(command, eventData.Duration);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        DbDataReader result, 
        CancellationToken cancellationToken = default)
    {
        CheckSlowQuery(command, eventData.Duration);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        int result)
    {
        CheckSlowQuery(command, eventData.Duration);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        int result, 
        CancellationToken cancellationToken = default)
    {
        CheckSlowQuery(command, eventData.Duration);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        object? result)
    {
        CheckSlowQuery(command, eventData.Duration);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        object? result, 
        CancellationToken cancellationToken = default)
    {
        CheckSlowQuery(command, eventData.Duration);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void LogCommand(DbCommand command)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        var commandText = ProcessCommandText(command.CommandText);

        _logger.LogDebug("Executing EF Core SQL Query: {CommandText}", commandText);
    }

    private void CheckSlowQuery(DbCommand command, TimeSpan duration)
    {
        if (duration > _options.SlowQueryThreshold)
        {
            var commandText = ProcessCommandText(command.CommandText);
            
            _logger.LogWarning("Slow EF Core SQL Query detected. Duration: {Duration}ms. Query: {CommandText}", 
                duration.TotalMilliseconds, 
                commandText);
        }
    }

    private string ProcessCommandText(string originalText)
    {
        if (!_options.MaskSensitiveData)
        {
            return originalText;
        }

        return PiiRegex().Replace(originalText, "***");
    }

    [GeneratedRegex(@"(password|secret|token)\s*=\s*'[^']*'", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PiiRegex();
}
