using Microsoft.EntityFrameworkCore;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Defines a contract for accessing the active <see cref="DbContext"/> within the current execution scope.
/// Allows non-repository components to safely access the context when required.
/// </summary>
public interface IVKDbContextAccessor
{
    /// <summary>
    /// Gets the current active <see cref="DbContext"/> in scope, or <c>null</c> if not initialized.
    /// </summary>
    DbContext? CurrentContext { get; }
}

/// <summary>
/// Defines a contract for accessing a typed <see cref="DbContext"/> within the current execution scope.
/// </summary>
/// <typeparam name="TDbContext">The specific DbContext type.</typeparam>
public interface IVKDbContextAccessor<out TDbContext> : IVKDbContextAccessor
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets the current active typed <typeparamref name="TDbContext"/> in scope, or <c>null</c> if not initialized.
    /// </summary>
    new TDbContext? CurrentContext { get; }
}
