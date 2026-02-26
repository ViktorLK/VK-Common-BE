using System.Diagnostics;
using VK.Blocks.Observability.Conventions;

namespace VK.Blocks.Observability.Enrichment;

/// <summary>
/// <see cref="ILogContextEnricher"/> の既定実装。
/// <para>
/// <see cref="Activity.Current"/> が存在する場合に
/// <see cref="FieldNames.TraceId"/> および <see cref="FieldNames.SpanId"/> を
/// <see cref="ILogEnricher"/> 群を通じてログコンテキストへプッシュする。
/// </para>
/// <para>
/// Serilog を使用する場合は、<c>LogContext.PushProperty</c> ベースの
/// <c>SerilogLogContextEnricher</c> をホスト側で提供すること。
/// 本クラスはプッシュされたプロパティを辞書で保持し、
/// <see cref="Dispose"/> 時にクリーンアップする。
/// </para>
/// </summary>
public sealed class ActivityLogContextEnricher : ILogContextEnricher
{
    #region Fields

    private readonly IEnumerable<ILogEnricher> _logEnrichers;

    #endregion

    #region Constructor

    /// <summary>
    /// <see cref="ActivityLogContextEnricher"/> の新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="logEnrichers">DI によって提供されるエンリッチャー群。</param>
    public ActivityLogContextEnricher(IEnumerable<ILogEnricher> logEnrichers)
    {
        _logEnrichers = logEnrichers;
    }

    #endregion

    #region ILogContextEnricher

    /// <inheritdoc />
    /// <remarks>
    /// <see cref="Activity.Current"/> が <c>null</c> の場合は <see cref="NullScope.Instance"/> を返し、
    /// アロケーションを最小化する。
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
    /// スコープ内に収集されたログプロパティを保持する内部スコープ実装。
    /// </summary>
    private sealed class LogScope : IDisposable
    {
        private readonly Dictionary<string, object?> _properties;
        private bool _disposed;

        internal LogScope(Dictionary<string, object?> properties)
        {
            _properties = properties;
        }

        /// <summary>収集されたプロパティへの読み取りアクセスを提供する（テスト・デバッグ用）。</summary>
        public IReadOnlyDictionary<string, object?> Properties => _properties;

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
    }

    /// <summary>
    /// アクティブな <see cref="Activity"/> が存在しない場合に返すノーオペレーション実装。
    /// </summary>
    private sealed class NullScope : IDisposable
    {
        /// <summary>シングルトンインスタンス。アロケーションを防ぐ。</summary>
        internal static readonly NullScope Instance = new();

        private NullScope() { }

        /// <inheritdoc />
        public void Dispose() { }
    }

    #endregion
}
