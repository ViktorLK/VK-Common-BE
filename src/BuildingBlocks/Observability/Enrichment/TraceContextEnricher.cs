using System.Diagnostics;
using VK.Blocks.Observability.Conventions;

namespace VK.Blocks.Observability.Enrichment;

/// <summary>
/// 現在のトレースコンテキスト (TraceId, SpanId) をログコンテキストに追加するエンリッチャー。
/// アクティブな <see cref="Activity"/> が存在する場合のみプロパティを追加する。
/// </summary>
public class TraceContextEnricher : ILogEnricher
{
    /// <inheritdoc />
    public void Enrich(Action<string, object?> propertyAdder)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            propertyAdder(FieldNames.TraceId, activity.TraceId.ToHexString());
            propertyAdder(FieldNames.SpanId, activity.SpanId.ToHexString());
        }
    }
}
