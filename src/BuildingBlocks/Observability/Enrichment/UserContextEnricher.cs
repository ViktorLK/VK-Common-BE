using Microsoft.Extensions.Options;
using VK.Blocks.Core.Context;
using VK.Blocks.Observability.Conventions;
using VK.Blocks.Observability.Options;

namespace VK.Blocks.Observability.Enrichment;

/// <summary>
/// An enricher that adds the authenticated user's identity (UserId, and optionally UserName)
/// to the log context.
/// UserName output is controlled by <see cref="ObservabilityOptions.IncludeUserName"/> for PII protection.
/// </summary>
public sealed class UserContextEnricher(IUserContext userContext, IOptions<ObservabilityOptions> options) : ILogEnricher
{
    #region Fields

    private readonly IUserContext _userContext = userContext;
    private readonly ObservabilityOptions _options = options.Value;

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
