using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Predefined error constants for VectorStore VecEngine operations [RuleID: CS.01].
/// </summary>
public static class VKVecEngineErrors
{
    private const string Prefix = "VectorStore.VecEngine.";

    public static readonly VKError MissingTenantId =
        VKError.Validation(Prefix + "MissingTenantId", "TenantId is required for all vector operations.");

    public static readonly VKError DimensionMismatch =
        VKError.Validation(Prefix + "DimensionMismatch", "Vector dimension does not match the target collection configuration.");

    public static readonly VKError LifecycleNotSupported =
        VKError.Conflict(Prefix + "LifecycleNotSupported", "Collection lifecycle operations (Create/Drop/Alias) are not supported on this backend role.");

    public static readonly VKError SchemaValidationFailed =
        VKError.Failure(Prefix + "SchemaValidationFailed", "Vector schema, extension, or index validation failed at startup.");

    public static readonly VKError MultiVectorNotSupported =
        VKError.Validation(Prefix + "MultiVectorNotSupported", "The target vector store does not support token-level multi-vector operations.");

    public static readonly VKError SparseVectorNotSupported =
        VKError.Validation(Prefix + "SparseVectorNotSupported", "The target vector store does not support sparse vector operations.");

    public static readonly VKError CollectionNotFound =
        VKError.NotFound(Prefix + "CollectionNotFound", "The specified vector collection does not exist.");

    public static readonly VKError DocumentNotFound =
        VKError.NotFound(Prefix + "DocumentNotFound", "The requested vector document was not found.");

    public static readonly VKError ExternalFailure =
        VKError.Failure(Prefix + "ExternalFailure", "External vector engine communication failed.");
}

