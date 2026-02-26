using Microsoft.Extensions.Options;
using VK.Blocks.Core.Context;
using VK.Blocks.Observability.Conventions;
using VK.Blocks.Observability.Options;

namespace VK.Blocks.Observability.Enrichment;

/// <summary>
/// 認証済みユーザーの識別情報 (ユーザーID、オプションでユーザー名) をログコンテキストに追加するエンリッチャー。
/// ユーザー名の出力は <see cref="ObservabilityOptions.IncludeUserName"/> で制御される (PII 保護)。
/// </summary>
public class UserContextEnricher(IUserContext userContext, IOptions<ObservabilityOptions> options) : ILogEnricher
{
    private readonly ObservabilityOptions _options = options.Value;

    /// <inheritdoc />
    public void Enrich(Action<string, object?> propertyAdder)
    {
        if (userContext.IsAuthenticated)
        {
            propertyAdder(FieldNames.UserId, userContext.UserId);

            if (_options.IncludeUserName)
            {
                propertyAdder(FieldNames.UserName, userContext.UserName);
            }
        }
    }
}
