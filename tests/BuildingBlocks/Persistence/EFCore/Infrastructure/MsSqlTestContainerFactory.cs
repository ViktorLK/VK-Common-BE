using System.Threading.Tasks;
using Testcontainers.MsSql;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;

/// <summary>
/// A factory for creating and managing SQL Server test containers.
/// </summary>
public static class MsSqlTestContainerFactory
{
    // Rationale: Using static readonly for the container to maintain a single instance for the test session if needed.
    private static readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    /// <summary>
    /// Gets the connection string for the SQL Server test container.
    /// </summary>
    public static string ConnectionString => _container.GetConnectionString();

    /// <summary>
    /// Initializes the SQL Server test container.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    /// <summary>
    /// Disposes of the SQL Server test container.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
