using VK.Blocks.Core;
using VK.Blocks.Testing.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Builders;

/// <summary>
/// Builder for constructing <see cref="VKPersonaAnchor"/> objects in unit tests.
/// </summary>
public sealed class VKPersonaAnchorBuilder : VKTestDataBuilder<VKPersonaAnchor>
{
    private VKPersonaId _id = new(Guid.NewGuid());
    private string _name = "Default Persona";
    private string _description = "Default Persona Description";
    private Dictionary<string, string> _traits = new();
    private Dictionary<string, object> _extensions = new();

    public VKPersonaAnchorBuilder WithId(VKPersonaId id)
    {
        _id = id;
        return this;
    }

    public VKPersonaAnchorBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public VKPersonaAnchorBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public VKPersonaAnchorBuilder WithTrait(string key, string value)
    {
        _traits[key] = value;
        return this;
    }

    public VKPersonaAnchorBuilder WithTraits(IReadOnlyDictionary<string, string> traits)
    {
        _traits = new Dictionary<string, string>(traits);
        return this;
    }

    public VKPersonaAnchorBuilder WithExtension(string key, object value)
    {
        _extensions[key] = value;
        return this;
    }

    protected override VKPersonaAnchor CreateDefault()
    {
        return VKGuard.NotNull(VKPersonaAnchor.Create(
            _id,
            _name,
            _description,
            _traits,
            _extensions).Value);
    }
}
