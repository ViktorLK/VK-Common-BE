namespace VK.Blocks.Core;

/// <summary>
/// Full audit contract combining creation, modification, deletion metadata, and soft delete state.
/// Implements <see cref="IVKAuditable"/> and <see cref="IVKSoftDeleteAudited"/>.
/// Follows AP.01.
/// </summary>
public interface IVKFullAuditable : IVKAuditable, IVKSoftDeleteAudited
{
}
