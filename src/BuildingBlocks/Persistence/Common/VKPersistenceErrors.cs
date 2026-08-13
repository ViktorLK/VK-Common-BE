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

        /// <summary>
        /// Error when entity already exists.
        /// </summary>
        public static readonly VKError EntityAlreadyExists = VKError.Conflict(
            "Persistence.Repository.EntityAlreadyExists",
            "The entity already exists in the data store.");

        /// <summary>
        /// Error when repository operation fails.
        /// </summary>
        public static readonly VKError OperationFailed = VKError.Failure(
            "Persistence.Repository.OperationFailed",
            "An error occurred while performing repository operation.");
    }

    /// <summary>
    /// Errors relating to database connectivity, execution, and timeouts.
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
        /// Error when execution of a database operation fails.
        /// </summary>
        public static readonly VKError ExecutionFailed = VKError.Failure(
            "Persistence.Database.ExecutionFailed",
            "An error occurred while executing the database operation.");

        /// <summary>
        /// Error when database operation times out.
        /// </summary>
        public static readonly VKError Timeout = VKError.Failure(
            "Persistence.Database.Timeout",
            "Database operation timed out.");

        /// <summary>
        /// Error when database deadlock occurs.
        /// </summary>
        public static readonly VKError Deadlock = VKError.Conflict(
            "Persistence.Database.Deadlock",
            "A database deadlock was detected.");

        /// <summary>
        /// Error for database constraint violations.
        /// </summary>
        public static readonly VKError ConstraintViolation = VKError.Conflict(
            "Persistence.Database.ConstraintViolation",
            "A database constraint was violated.");
    }

    /// <summary>
    /// Errors relating to database constraint violations.
    /// </summary>
    public static class Constraint
    {
        /// <summary>
        /// Error when a unique constraint is violated.
        /// </summary>
        public static readonly VKError UniqueViolation = VKError.Conflict(
            "Persistence.Constraint.UniqueViolation",
            "A unique constraint violation occurred in the data store.");

        /// <summary>
        /// Error when a foreign key constraint is violated.
        /// </summary>
        public static readonly VKError ForeignKeyViolation = VKError.Conflict(
            "Persistence.Constraint.ForeignKeyViolation",
            "A foreign key constraint violation occurred in the data store.");
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
