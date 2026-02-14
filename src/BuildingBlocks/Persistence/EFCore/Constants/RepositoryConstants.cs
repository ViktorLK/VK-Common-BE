namespace VK.Blocks.Persistence.EFCore.Constants;

/// <summary>
/// Constants for the repository layer.
/// Features: Centralized management of string literals for improved readability and maintainability.
/// </summary>
public static class RepositoryConstants
{
    /// <summary>
    /// Error messages.
    /// </summary>
    public static class ErrorMessages
    {
        #region Constants

        /// <summary>
        /// Page number must be greater than 0.
        /// </summary>
        public const string PageNumberMustBePositive = "Page number must be greater than 0.";

        /// <summary>
        /// Page size must be greater than 0.
        /// </summary>
        public const string PageSizeMustBePositive = "Page size must be greater than 0.";

        /// <summary>
        /// Offset pagination limit exceeded.
        /// </summary>
        public const string OverOffsetLimit = "Offset pagination limit exceeded.";

        /// <summary>
        /// Page size limit exceeded.
        /// </summary>
        public const string OverPageSize = "Page size limit exceeded.";

        /// <summary>
        /// Transaction is already active.
        /// </summary>
        public const string TransactionAlreadyActive = "Transaction is already active.";

        /// <summary>
        /// No active transaction to commit.
        /// </summary>
        public const string NoActiveTransaction = "No active transaction to commit.";

        /// <summary>
        /// Primary key not found (format with entity name).
        /// </summary>
        public const string PrimaryKeyNotFoundFormat = "Primary key not found for entity '{0}'. Use FindWithIncludesAsync.";

        #endregion
    }
}
