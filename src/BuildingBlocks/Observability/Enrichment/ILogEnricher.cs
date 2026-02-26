namespace VK.Blocks.Observability.Enrichment;

/// <summary>
/// ログイベントにカスタムプロパティを追加するエンリッチャーのインターフェース。
/// Strategy パターンにより、複数のエンリッチャーを組み合わせてログコンテキストを拡張する。
/// </summary>
public interface ILogEnricher
{
    /// <summary>
    /// ログイベントにプロパティを追加する。
    /// </summary>
    /// <param name="propertyAdder">
    /// プロパティ追加用デリゲート。第1引数はフィールド名 (例: <c>"service.name"</c>)、第2引数はその値。
    /// </param>
    void Enrich(Action<string, object?> propertyAdder);
}
