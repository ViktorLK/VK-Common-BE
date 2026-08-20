using System;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.Testing.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Builders;

/// <summary>
/// Builder for constructing <see cref="VKPsycheRequest"/> objects in unit tests.
/// </summary>
public sealed class VKPsycheRequestBuilder : VKTestDataBuilder<VKPsycheRequest>
{
    private VKPersonaId _personaId = new(Guid.NewGuid());
    private VKSessionId _sessionId = new(Guid.NewGuid());
    private VKProfileId? _profileId;
    private string _userInput = "hello";

    public VKPsycheRequestBuilder WithPersonaId(VKPersonaId personaId)
    {
        _personaId = personaId;
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

    public VKPsycheRequestBuilder WithUserInput(string userInput)
    {
        _userInput = userInput;
        return this;
    }

    protected override VKPsycheRequest CreateDefault()
    {
        return new VKPsycheRequest
        {
            PersonaIds = [_personaId],
            SessionId = _sessionId,
            ProfileId = _profileId,
            UserInput = _userInput
        };
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
