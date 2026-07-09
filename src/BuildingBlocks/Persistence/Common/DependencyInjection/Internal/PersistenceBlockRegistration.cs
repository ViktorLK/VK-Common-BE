namespace VK.Blocks.Persistence.Common.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the Persistence building block.
/// </summary>
// [SG Registration]
internal static partial class PersistenceBlockRegistration
{
    // [SG Hook]
    static partial void RegisterBlockCustom(IVKPersistenceBuilder builder)
    {
        PersistenceDefaultsFeature.Register(builder);
    }
}
