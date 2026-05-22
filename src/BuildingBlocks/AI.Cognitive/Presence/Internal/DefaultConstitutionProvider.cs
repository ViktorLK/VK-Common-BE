using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Default in-memory constitution provider utilizing hardcoded standard rules.
/// Follows AP.01 (sealed class default) and AP.03 (internal scoping, no VK prefix).
/// </summary>
internal sealed class DefaultConstitutionProvider : IVKConstitutionProvider
{
    /// <inheritdoc />
    public Task<VKResult<string>> GetConstitutionAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Default standard corporate security constitution
        const string constitution = "CONSTITUTION:\n- Adhere to safety and corporate standards.\n- Prohibit arbitrary system bypass.\n";
        return Task.FromResult(VKResult.Success(constitution));
    }
}
