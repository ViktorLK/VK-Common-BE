using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Testing.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Builders;

/// <summary>
/// Builder for constructing <see cref="VKPsycheRequest"/> objects in unit tests.
/// </summary>
public sealed class VKPsycheRequestBuilder : VKTestDataBuilder<VKPsycheRequest>
{
    private readonly List<VKPersonaId> _personaIds = [];
    private readonly List<VKDirectiveId> _directiveIds = [];
    private readonly List<VKPatternId> _patternIds = [];
    private VKSessionId _sessionId = new(Guid.NewGuid());
    private VKProfileId? _profileId;
    private readonly List<VKKnowledgeId> _knowledgeIds = [];
    private string _userInput = "hello";
    private bool _weaveOnly;
    private string? _correlationId;
    private readonly List<Func<VKPsycheRequest, VKPsycheRequest>> _argsConfigurators = [];

    public VKPsycheRequestBuilder WithPersonaId(VKPersonaId personaId)
    {
        _personaIds.Add(personaId);
        return this;
    }

    public VKPsycheRequestBuilder WithPersonaIds(IEnumerable<VKPersonaId> personaIds)
    {
        _personaIds.AddRange(personaIds);
        return this;
    }

    public VKPsycheRequestBuilder WithDirectiveId(VKDirectiveId directiveId)
    {
        _directiveIds.Add(directiveId);
        return this;
    }

    public VKPsycheRequestBuilder WithDirectiveIds(IEnumerable<VKDirectiveId> directiveIds)
    {
        _directiveIds.AddRange(directiveIds);
        return this;
    }

    public VKPsycheRequestBuilder WithPatternId(VKPatternId patternId)
    {
        _patternIds.Add(patternId);
        return this;
    }

    public VKPsycheRequestBuilder WithPatternIds(IEnumerable<VKPatternId> patternIds)
    {
        _patternIds.AddRange(patternIds);
        return this;
    }

    public VKPsycheRequestBuilder WithSessionId(VKSessionId sessionId)
    {
        _sessionId = sessionId;
        return this;
    }

    public VKPsycheRequestBuilder WithProfileId(VKProfileId profileId)
    {
        _profileId = profileId;
        return this;
    }

    public VKPsycheRequestBuilder WithKnowledgeId(VKKnowledgeId knowledgeId)
    {
        _knowledgeIds.Add(knowledgeId);
        return this;
    }

    public VKPsycheRequestBuilder WithKnowledgeIds(IEnumerable<VKKnowledgeId> knowledgeIds)
    {
        _knowledgeIds.AddRange(knowledgeIds);
        return this;
    }

    public VKPsycheRequestBuilder WithUserInput(string userInput)
    {
        _userInput = userInput;
        return this;
    }

    public VKPsycheRequestBuilder WithWeaveOnly(bool weaveOnly = true)
    {
        _weaveOnly = weaveOnly;
        return this;
    }

    public VKPsycheRequestBuilder WithCorrelationId(string correlationId)
    {
        _correlationId = correlationId;
        return this;
    }

    public VKPsycheRequestBuilder WithRequestArgs<TArgs>(TArgs args) where TArgs : class
    {
        _argsConfigurators.Add(req => req.WithArgs(args));
        return this;
    }

    protected override VKPsycheRequest CreateDefault()
    {
        var req = new VKPsycheRequest
        {
            PersonaIds = _personaIds.Count > 0 ? [.. _personaIds] : [new VKPersonaId(Guid.NewGuid())],
            DirectiveIds = [.. _directiveIds],
            PatternIds = [.. _patternIds],
            SessionId = _sessionId,
            ProfileId = _profileId,
            KnowledgeIds = [.. _knowledgeIds],
            UserInput = _userInput,
            WeaveOnly = _weaveOnly,
            CorrelationId = _correlationId
        };

        foreach (var configure in _argsConfigurators)
        {
            req = configure(req);
        }

        return req;
    }

    /// <summary>
    /// Builds both the <see cref="VKPsycheContext"/> and the underlying <see cref="IServiceProvider"/>.
    /// </summary>
    public (VKPsycheContext Context, IServiceProvider Services) BuildContext(IServiceCollection? serviceCollection = null)
    {
        var request = Build();
        var services = (serviceCollection ?? new ServiceCollection()).BuildServiceProvider();

        var context = new VKPsycheContext
        {
            Request = request,
            CorrelationId = Guid.NewGuid().ToString(),
            CreatedAt = DateTimeOffset.UtcNow,
            Services = services
        };

        return (context, services);
    }
}
