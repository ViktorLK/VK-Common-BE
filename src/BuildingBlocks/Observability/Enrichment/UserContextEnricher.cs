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
    #region Fields

    private readonly IUserContext _userContext = userContext;
    private readonly ObservabilityOptions _options = options.Value;

    #endregion
    #region Constructors

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public void Enrich(Action<string, object?> propertyAdder)
    {
        if (_userContext.IsAuthenticated)
        {
            propertyAdder(FieldNames.UserId, _userContext.UserId);

            if (_options.IncludeUserName)
            {
                propertyAdder(FieldNames.UserName, _userContext.UserName);
            }
        }
    }

    #endregion
}
