using System;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.Common.Internal;

/// <summary>
/// Exception thrown when a resilience operation fails.
/// </summary>
internal sealed class ResilienceException : VKBaseException
{
    private const string DefaultCode = "Resilience.Execution.Failed";

    public ResilienceException(string message)
        : base(DefaultCode, message)
    {
    }

    public ResilienceException(string message, Exception innerException)
        : base(DefaultCode, message)
    {
    }
}
