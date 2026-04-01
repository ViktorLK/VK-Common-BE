using VK.Blocks.Core.Exceptions;

namespace VK.Blocks.Mapping.Exceptions;

/// <summary>
/// Exception thrown when a mapping error occurs.
/// </summary>
public sealed class MappingException : BaseException
{
    public MappingException(string message) 
        : base("MAPPING_ERROR", message, 500)
    {
    }

    public MappingException(string message, Exception innerException) 
        : base("MAPPING_ERROR", $"{message} (Inner: {innerException.Message})", 500)
    {
    }
}
