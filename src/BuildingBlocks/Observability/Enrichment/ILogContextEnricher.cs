namespace VK.Blocks.Observability.Enrichment;

/// <summary>
/// アクティブな <see cref="System.Diagnostics.Activity"/> のトレースコンテキスト
/// (TraceId・SpanId) をログコンテキストへ自動プッシュするための抽象化。
/// <para>
/// この抽象化は Serilog に依存しない。具体実装はホスト側のロギングプロバイダー
/// (Serilog LogContext、Microsoft.Extensions.Logging Scope 等) に合わせて選択する。
/// </para>
/// </summary>
public interface ILogContextEnricher
{
    /// <summary>
    /// ログコンテキストを拡張したスコープを開始する。
    /// </summary>
    /// <returns>
    /// スコープの終了時に Dispose することでログコンテキストをクリーンアップする
    /// <see cref="IDisposable"/>。
    /// アクティブな <see cref="System.Diagnostics.Activity"/> が存在しない場合は
    /// ノーオペレーション実装を返す。
    /// </returns>
    IDisposable Enrich();
}
