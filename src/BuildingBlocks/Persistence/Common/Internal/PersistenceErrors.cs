using VK.Blocks.Core;

namespace VK.Blocks.Persistence.Common.Internal;

/// <summary>
/// Domain-specific error constants for the Persistence building block.
/// Follows CS.01: {ModuleName}.{Category}.{Reason} format.
/// </summary>
internal static class PersistenceErrors
{
    public static class UnitOfWork
    {
        public static readonly VKError SaveChangesFailed = VKError.Failure(
            "Persistence.UnitOfWork.SaveChangesFailed",
            "An error occurred while saving changes to the data store.");
            
        public static readonly VKError ConcurrentUpdate = VKError.Conflict(
            "Persistence.UnitOfWork.ConcurrentUpdate",
            "The entity has been modified by another process.");
    }

    public static class Repository
    {
        public static readonly VKError EntityNotFound = VKError.NotFound(
            "Persistence.Repository.EntityNotFound",
            "The requested entity was not found.");
    }
}

