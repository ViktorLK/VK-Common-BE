using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Database.Internal;

/// <summary>
/// Default scoped accessor that resolves the active <see cref="DbContext"/> from the service provider.
/// Follows AP.01, AP.03.
/// </summary>
internal sealed class DefaultDbContextAccessor(IServiceProvider serviceProvider) : IVKDbContextAccessor
{
    private readonly IServiceProvider _serviceProvider = VKGuard.NotNull(serviceProvider);

    /// <inheritdoc />
    public DbContext? CurrentContext => _serviceProvider.GetService<DbContext>();
}

/// <summary>
/// Default scoped accessor that resolves the active typed <typeparamref name="TDbContext"/> from the service provider.
/// Follows AP.01, AP.03.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
internal sealed class DefaultDbContextAccessor<TDbContext>(IServiceProvider serviceProvider) : IVKDbContextAccessor<TDbContext>
    where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider = VKGuard.NotNull(serviceProvider);

    /// <inheritdoc />
    public TDbContext? CurrentContext => _serviceProvider.GetService<TDbContext>();

    DbContext? IVKDbContextAccessor.CurrentContext => CurrentContext;
}
