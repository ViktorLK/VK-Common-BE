using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Knowledge.Internal;

/// <summary>
/// Default implementation of <see cref="IVKKnowledgeSessionProvider"/> falling back to user context.
/// </summary>
internal sealed class BasicKnowledgeSessionProvider : IVKKnowledgeSessionProvider
{
    private readonly IVKUserContext _userContext;

    public BasicKnowledgeSessionProvider(IVKUserContext userContext)
    {
        _userContext = VKGuard.NotNull(userContext);
    }

    public string GetCurrentSessionId()
    {
        return _userContext.UserId ?? "ambient_default_session";
    }
}
