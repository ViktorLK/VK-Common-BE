namespace VK.Blocks.Observability.OpenTelemetry.Processors;

/// <summary>
/// OpenTelemetry エクスポートプロセッサの動作モードを定義する列挙型。
/// <para>
/// <list type="table">
///   <listheader><term>値</term><description>説明 / 用途</description></listheader>
///   <item>
///     <term><see cref="Batch"/></term>
///     <description>
///       スパンをバッファリングしてバッチ送信する。本番環境のデフォルト。
///       CPU・ネットワーク効率に優れる。
///     </description>
///   </item>
///   <item>
///     <term><see cref="Simple"/></term>
///     <description>
///       スパンを即座に同期エクスポートする。ローカル開発・デバッグ向け。
///       レイテンシが発生するため本番環境では非推奨。
///     </description>
///   </item>
/// </list>
/// </para>
/// </summary>
public enum ProcessorMode
{
    /// <summary>
    /// バッチエクスポートプロセッサ（本番環境推奨）。
    /// スパンをキューに蓄積し、設定された間隔でまとめて送信する。
    /// </summary>
    Batch = 0,

    /// <summary>
    /// シンプルエクスポートプロセッサ（開発・デバッグ向け）。
    /// スパン完了直後に即座にエクスポートする。
    /// </summary>
    Simple = 1
}
