using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Common.Internal;

/// <summary>
/// Domain-specific error constants for the EF Core Persistence module.
/// Follows CS.01: {ModuleName}.{Category}.{Reason} format.
/// </summary>
internal static class EfCoreErrors
{
    public static class Transaction
    {
        public static readonly VKError AlreadyActive = VKError.Failure(
            "Persistence.EFCore.Transaction.AlreadyActive",
            "A transaction is already active for this Unit of Work.");

        public static readonly VKError NoActive = VKError.Failure(
            "Persistence.EFCore.Transaction.NoActive",
            "No active transaction was found to commit or rollback.");
    }

    public static class Repository
    {
        public static readonly VKError PrimaryKeyNotFound = VKError.NotFound(
            "Persistence.EFCore.Repository.PrimaryKeyNotFound",
            "The primary key for the specified entity could not be found.");
            
        public static readonly VKError EntityNotFound = VKError.NotFound(
            "Persistence.EFCore.Repository.EntityNotFound",
            "The requested entity was not found in the database.");
    }

    public static class Pagination
    {
        public static readonly VKError InvalidPageNumber = VKError.Validation(
            "Persistence.EFCore.Pagination.InvalidPageNumber",
            "Page number must be greater than zero.");

        public static readonly VKError InvalidPageSize = VKError.Validation(
            "Persistence.EFCore.Pagination.InvalidPageSize",
            "Page size must be greater than zero.");
            
        public static readonly VKError PageSizeLimitExceeded = VKError.Validation(
            "Persistence.EFCore.Pagination.PageSizeLimitExceeded",
            "The requested page size exceeds the maximum allowed limit.");
    }
}

