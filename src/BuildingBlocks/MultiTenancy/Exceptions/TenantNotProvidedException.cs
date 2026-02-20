using VK.Blocks.Core.Exceptions;
using VK.Blocks.MultiTenancy.Constants;

namespace VK.Blocks.MultiTenancy.Exceptions;

/// <summary>
/// Exception thrown when a tenant identifier is required but not provided.
/// </summary>
public sealed class TenantNotProvidedException : BaseException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantNotProvidedException"/> class.
    /// </summary>
    public TenantNotProvidedException()
        : base(MultiTenancyConstants.Errors.MissingTenantCode, MultiTenancyConstants.Errors.MissingTenantMessage, 401)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantNotProvidedException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TenantNotProvidedException(string message)
        : base(MultiTenancyConstants.Errors.MissingTenantCode, message, 401)
    {
    }
}
