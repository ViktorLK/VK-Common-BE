using VK.Blocks.Core;

namespace VK.Blocks.Persistence;

/// <summary>
/// Domain-specific error constants for the Persistence building block.
/// Follows CS.01: {ModuleName}.{Category}.{Reason} format.
/// </summary>
public static class VKPersistenceErrors
{
    /// <summary>
    /// Errors relating to Unit of Work operations.
    /// </summary>
    public static class UnitOfWork
    {
        /// <summary>
        /// Error when SaveChanges fails.
        /// </summary>
        public static readonly VKError SaveChangesFailed = VKError.Failure(
            "Persistence.UnitOfWork.SaveChangesFailed",
            "An error occurred while saving changes to the data store.");
            
        /// <summary>
        /// Error for optimistic concurrency conflicts.
        /// </summary>
        public static readonly VKError ConcurrentUpdate = VKError.Conflict(
            "Persistence.UnitOfWork.ConcurrentUpdate",
            "The entity has been modified by another process.");
    }

    /// <summary>
    /// Errors relating to Repository operations.
    /// </summary>
    public static class Repository
    {
        /// <summary>
        /// Error when entity is not found.
        /// </summary>
        public static readonly VKError EntityNotFound = VKError.NotFound(
            "Persistence.Repository.EntityNotFound",
            "The requested entity was not found.");
    }

    /// <summary>
    /// Errors relating to database connectivity and constraints.
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Error when connection to database fails.
        /// </summary>
        public static readonly VKError ConnectionFailed = VKError.Failure(
            "Persistence.Database.ConnectionFailed",
            "Unable to establish connection to the database.");

        /// <summary>
        /// Error for database constraint violations.
        /// </summary>
        public static readonly VKError ConstraintViolation = VKError.Conflict(
            "Persistence.Database.ConstraintViolation",
            "A database constraint was violated.");
    }

    /// <summary>
    /// Errors relating to Transaction operations.
    /// </summary>
    public static class Transaction
    {
        /// <summary>
        /// Error when beginning a transaction fails.
        /// </summary>
        public static readonly VKError BeginFailed = VKError.Failure(
            "Persistence.Transaction.BeginFailed",
            "Failed to begin a database transaction.");

        /// <summary>
        /// Error when committing a transaction fails.
        /// </summary>
        public static readonly VKError CommitFailed = VKError.Failure(
            "Persistence.Transaction.CommitFailed",
            "Failed to commit the database transaction.");

        /// <summary>
        /// Error when a transaction is already active.
        /// </summary>
        public static readonly VKError AlreadyActive = VKError.Conflict(
            "Persistence.Transaction.AlreadyActive",
            "A transaction is already active on this unit of work.");

        /// <summary>
        /// Error when no active transaction exists.
        /// </summary>
        public static readonly VKError NoActiveTransaction = VKError.Failure(
            "Persistence.Transaction.NoActiveTransaction",
            "No active transaction to commit or rollback.");
    }

    /// <summary>
    /// Errors relating to database and system health.
    /// </summary>
    public static class Health
    {
        /// <summary>
        /// Error when the database connection is unhealthy.
        /// </summary>
        public static readonly VKError ConnectionUnhealthy = VKError.Failure(
            "Persistence.Health.ConnectionUnhealthy",
            "The database connection health check failed.");

        /// <summary>
        /// Error when there are pending migrations.
        /// </summary>
        public static readonly VKError MigrationPending = VKError.Failure(
            "Persistence.Health.MigrationPending",
            "There are pending database migrations.");
    }
}
