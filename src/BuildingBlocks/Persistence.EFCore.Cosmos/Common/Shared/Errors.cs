using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Cosmos;

/// <summary>
/// Internal error registry for the Cosmos building block.
/// </summary>
internal static class Errors
{
    public static class Connection
    {
        public static VKError InitializationFailed(string message) =>
            VKError.Failure("Persistence.Cosmos.Connection.InitializationFailed", $"Cosmos DB initialization failed: {message}");
    }

    public static class Concurrency
    {
        public static VKError PreconditionFailed(string message) =>
            VKError.PreconditionFailed("Persistence.Cosmos.Concurrency.PreconditionFailed", $"Cosmos DB pre-condition check failed: {message}");
    }

    public static class Batch
    {
        public static VKError OperationLimitExceeded(int count) =>
            VKError.Validation("Persistence.Cosmos.Batch.OperationLimitExceeded", $"Transactional batch cannot exceed 100 operations. Requested: {count}");

        public static VKError ExecutionFailed(string message) =>
            VKError.Failure("Persistence.Cosmos.Batch.ExecutionFailed", $"Transactional batch execution failed: {message}");
    }

    public static class Session
    {
        public static VKError TokenPropagationFailed(string message) =>
            VKError.Failure("Persistence.Cosmos.Session.TokenPropagationFailed", $"Session token propagation failed: {message}");
    }

    public static class Query
    {
        public static VKError ExecutionFailed(string message) =>
            VKError.Failure("Persistence.Cosmos.Query.ExecutionFailed", $"Query execution failed: {message}");

        public static readonly VKError MultipleElementsFound =
            VKError.Validation("Persistence.Cosmos.Query.MultipleElementsFound", "More than one element satisfies the condition in SingleOrDefault.");
    }

    public static class ServerSide
    {
        public static VKError RegistrationFailed(string type, string id, string message) =>
            VKError.Failure($"Persistence.Cosmos.ServerSide.{type}RegistrationFailed", $"{type} '{id}' registration failed: {message}");

        public static VKError ExecutionFailed(string id, string message) =>
            VKError.Failure("Persistence.Cosmos.ServerSide.ExecutionFailed", $"Stored procedure '{id}' execution failed: {message}");
    }

    public static class Provisioning
    {
        public static VKError ContainerCreationFailed(string containerName, string message) =>
            VKError.Failure("Persistence.Cosmos.Provisioning.ContainerCreationFailed", $"Container '{containerName}' creation failed: {message}");
    }

    public static class Failover
    {
        public static VKError ReadAccountFailed(string message) =>
            VKError.Failure("Persistence.Cosmos.Failover.ReadAccountFailed", $"Failed to read account info: {message}");
    }
}
