using VK.Blocks.Core.Exceptions;
using VK.Blocks.MultiTenancy.Constants;

namespace VK.Blocks.MultiTenancy.Exceptions;

/// <summary>
/// Exception thrown when a tenant identifier is required but not provided.
/// </summary>
public sealed class InvalidTenantImplementationException : BaseException
{
    public InvalidTenantImplementationException(string message)
        : base(MultiTenancyConstants.Errors.InvalidTenantImplementationCode, message, 500)
    {
    }
}
