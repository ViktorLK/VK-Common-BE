namespace VK.Blocks.MultiTenancy.Abstractions.Contracts;

/// <summary>
/// Represents the result of a tenant resolution attempt.
/// Provides factory methods for creating success and failure outcomes.
/// </summary>
public sealed record TenantResolutionResult
{
    #region Constructors

    private TenantResolutionResult(bool isSuccess, string? tenantId, string? error)
    {
        IsSuccess = isSuccess;
        TenantId = tenantId;
        Error = error;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether the tenant was successfully resolved.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the resolved tenant identifier, or <c>null</c> if resolution failed.
    /// </summary>
    public string? TenantId { get; }

    /// <summary>
    /// Gets the error description if resolution failed, or <c>null</c> if successful.
    /// </summary>
    public string? Error { get; }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a successful resolution result with the specified tenant identifier.
    /// </summary>
    /// <param name="tenantId">The resolved tenant identifier.</param>
    /// <returns>A successful <see cref="TenantResolutionResult"/>.</returns>
    public static TenantResolutionResult Success(string tenantId) =>
        new(true, tenantId, null);

    /// <summary>
    /// Creates a failed resolution result with the specified error description.
    /// </summary>
    /// <param name="error">The error description explaining why resolution failed.</param>
    /// <returns>A failed <see cref="TenantResolutionResult"/>.</returns>
    public static TenantResolutionResult Fail(string error) =>
        new(false, null, error);

    #endregion
}
