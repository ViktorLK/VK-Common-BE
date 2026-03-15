using System.Diagnostics;
using VK.Blocks.Observability.Conventions;

namespace VK.Blocks.Observability.Enrichment;

/// <summary>
/// Default implementation of <see cref="ILogContextEnricher"/>.
/// <para>
/// If <see cref="Activity.Current"/> exists, it pushes <see cref="FieldNames.TraceId"/>
/// and <see cref="FieldNames.SpanId"/> to the log context via the provided <see cref="ILogEnricher"/>s.
/// </para>
/// <para>
/// For Serilog, a <c>SerilogLogContextEnricher</c> based on <c>LogContext.PushProperty</c>
/// should be provided at the host level. This class holds pushed properties in a dictionary
/// and handles cleanup during <see cref="Dispose"/>.
/// </para>
/// </summary>
public sealed class ActivityLogContextEnricher : ILogContextEnricher
{
    #region Fields

    private readonly IEnumerable<ILogEnricher> _logEnrichers;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityLogContextEnricher"/> class.
    /// </summary>
    /// <param name="logEnrichers">Enrichers provided via dependency injection.</param>
    public ActivityLogContextEnricher(IEnumerable<ILogEnricher> logEnrichers)
    {
        _logEnrichers = logEnrichers;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    /// <remarks>
    /// Returns <see cref="NullScope.Instance"/> if <see cref="Activity.Current"/> is <c>null</c>
    /// to minimize allocations.
    /// </remarks>
    public IDisposable Enrich()
    {
        var activity = Activity.Current;
        if (activity is null)
        {
            return NullScope.Instance;
        }

        var properties = new Dictionary<string, object?>(capacity: 4);

        foreach (var enricher in _logEnrichers)
        {
            enricher.Enrich((key, value) => properties[key] = value);
        }

        return new LogScope(properties);
    }

    #endregion

    #region Nested Types

    /// <summary>
    /// Implementation of an internal scope that holds log properties collected within the scope.
    /// </summary>
    private sealed class LogScope : IDisposable
    {
        #region Fields

        private readonly Dictionary<string, object?> _properties;
        private bool _disposed;

        #endregion

        #region Properties

        /// <summary>Gets the collected properties (for testing and debugging purposes).</summary>
        public IReadOnlyDictionary<string, object?> Properties => _properties;

        #endregion

        #region Constructors

        internal LogScope(Dictionary<string, object?> properties)
        {
            _properties = properties;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _properties.Clear();
            _disposed = true;
        }

        #endregion
    }

    /// <summary>
    /// No-op implementation returned when no active <see cref="Activity"/> exists.
    /// </summary>
    private sealed class NullScope : IDisposable
    {
        #region Fields

        /// <summary>The singleton instance to prevent redundant allocations.</summary>
        internal static readonly NullScope Instance = new();

        #endregion

        #region Constructors

        private NullScope() { }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public void Dispose() { }

        #endregion
    }

    #endregion
}
