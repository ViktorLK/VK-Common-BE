namespace VK.Blocks.Core.Exceptions;

/// <summary>
/// Abstract base for all application exceptions.
/// Provides a machine-readable <see cref="Code"/> alongside the human-readable message.
/// CRTP pattern
/// </summary>
public abstract class BaseException<TException>(
    string code,
    string message,
    int statusCode = 400,
    bool isPublic = true) : Exception(message)
    where TException : BaseException<TException>
{
    public string Code { get; } = code ?? throw new ArgumentNullException(nameof(code));
    public int StatusCode { get; } = statusCode;
    public bool IsPublic { get; } = isPublic;
    public IReadOnlyDictionary<string, object?> Extensions => _extensions;

    private readonly Dictionary<string, object?> _extensions = [];

    public TException WithExtension(string key, object? value)
    {
        _extensions[key] = value;
        return (TException)this;
    }
}
