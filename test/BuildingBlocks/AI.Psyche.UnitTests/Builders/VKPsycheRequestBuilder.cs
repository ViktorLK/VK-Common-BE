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
    private VKTenantId _tenantId = VKTenantId.Default;
    private VKPersonaId _personaId = new(Guid.NewGuid());
    private VKSessionId _sessionId = new(Guid.NewGuid());
    private string _userInput = "hello";

    public VKPsycheRequestBuilder WithTenantId(VKTenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

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

    public VKPsycheRequestBuilder WithUserInput(string userInput)
    {
        _userInput = userInput;
        return this;
    }

    protected override VKPsycheRequest CreateDefault()
    {
        return new VKPsycheRequest
        {
            TenantId = _tenantId,
            PersonaId = _personaId,
            SessionId = _sessionId,
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
            Services = services
        };

        return (context, services);
    }
}
