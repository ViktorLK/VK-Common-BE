using VK.Blocks.Core;

namespace VK.Blocks.Testing;

/// <summary>
/// Deterministic Guid generator for unit tests where fixed Guids are needed.
/// </summary>
public sealed class VKFakeGuidGenerator : IVKGuidGenerator
{
    private readonly Queue<Guid> _predefinedGuids;
    private int _sequence;

    /// <summary>
    /// Initializes a new instance of <see cref="VKFakeGuidGenerator"/> with an optional sequence of fixed Guids.
    /// </summary>
    /// <param name="guids">Predefined sequence of Guids to return.</param>
    public VKFakeGuidGenerator(params Guid[] guids)
    {
        _predefinedGuids = new Queue<Guid>(guids);
    }

    /// <inheritdoc />
    public Guid Create()
    {
        if (_predefinedGuids.Count > 0)
        {
            return _predefinedGuids.Dequeue();
        }

        int next = Interlocked.Increment(ref _sequence);
        return new Guid(next, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }
}
