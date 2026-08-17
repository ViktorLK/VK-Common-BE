using Respawn;

namespace VK.Blocks.Testing.EntityFramework.SqlServer.Internal;

/// <summary>
/// Respawn-based database reset for SQL Server.
/// </summary>
internal sealed class VKRespawnDatabaseReset : IVKDatabaseReset
{
    private readonly Respawner _respawner;
    private readonly string _connectionString;

    private VKRespawnDatabaseReset(Respawner respawner, string connectionString)
    {
        _respawner = respawner;
        _connectionString = connectionString;
    }

    public static async Task<VKRespawnDatabaseReset> CreateAsync(
        string connectionString,
        RespawnerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = ["dbo"],
            TablesToIgnore = [new RespawnerOptions().TablesToIgnore.FirstOrDefault() ?? new Respawn.Graph.Table("__EFMigrationsHistory")]
        };

        var respawner = await Respawner.CreateAsync(connectionString, options).ConfigureAwait(false);
        return new VKRespawnDatabaseReset(respawner, connectionString);
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await _respawner.ResetAsync(_connectionString).ConfigureAwait(false);
    }
}
