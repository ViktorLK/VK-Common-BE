namespace VK.Blocks.Core;

/// <summary>
/// Composite audit contract combining creation and modification tracking.
/// Implements <see cref="IVKCreationAudited"/> and <see cref="IVKModificationAudited"/>.
/// Follows AP.01.
/// </summary>
public interface IVKAuditable : IVKCreationAudited, IVKModificationAudited
{
}
