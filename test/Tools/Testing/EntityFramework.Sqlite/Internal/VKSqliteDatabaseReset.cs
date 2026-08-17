using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace VK.Blocks.Testing.EntityFramework.Sqlite;

/// <summary>
/// Reset strategy for SQLite in-memory databases by executing DELETE FROM on all user tables.
/// </summary>
internal sealed class VKSqliteDatabaseReset : IVKDatabaseReset
{
    private readonly SqliteConnection _connection;

    public VKSqliteDatabaseReset(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        using var command = _connection.CreateCommand();
        command.CommandText = @"
            SELECT name FROM sqlite_master 
            WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '__EFMigrationsHistory';";

        var tables = new List<string>();
        using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                tables.Add(reader.GetString(0));
            }
        }

        if (tables.Count == 0) return;

        using var disableKeys = _connection.CreateCommand();
        disableKeys.CommandText = "PRAGMA foreign_keys = OFF;";
        await disableKeys.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        foreach (var table in tables)
        {
            using var deleteCmd = _connection.CreateCommand();
            deleteCmd.CommandText = $"DELETE FROM \"{table}\";";
            await deleteCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        using var enableKeys = _connection.CreateCommand();
        enableKeys.CommandText = "PRAGMA foreign_keys = ON;";
        await enableKeys.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
