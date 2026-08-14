using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Store interface for retrieving Prompt Presets in AI.Psyche.
/// </summary>
public interface IVKPromptPresetStore
{
    Task<VKResult<VKPromptPreset>> GetPresetAsync(string presetId, CancellationToken cancellationToken = default);
    Task<VKResult<IEnumerable<VKPromptPreset>>> GetPresetsAsync(CancellationToken cancellationToken = default);
}
