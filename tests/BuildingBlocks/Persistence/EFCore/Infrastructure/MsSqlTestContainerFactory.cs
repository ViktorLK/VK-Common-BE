using Testcontainers.MsSql;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;

public static class MsSqlTestContainerFactory
{
    private static readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public static string ConnectionString => _container.GetConnectionString();

    public static async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public static async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
