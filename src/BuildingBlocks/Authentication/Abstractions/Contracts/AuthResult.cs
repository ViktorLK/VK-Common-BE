using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Abstractions.Contracts;

/// <summary>
/// Represents the result of an authentication operation.
/// </summary>
public class AuthResult : Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthResult"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the authentication was successful.</param>
    /// <param name="error">The error associated with the authentication result, if any.</param>
    protected AuthResult(bool isSuccess, Error error) : base(isSuccess, error)
    {
    }
}
