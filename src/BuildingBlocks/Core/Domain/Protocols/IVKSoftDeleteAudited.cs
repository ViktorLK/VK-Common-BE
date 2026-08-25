namespace VK.Blocks.Core;

/// <summary>
/// Composite contract for entities supporting soft deletion with deletion audit tracking.
/// Combines <see cref="IVKSoftDeletable"/> (IsDeleted) and <see cref="IVKDeletionAudited"/> (DeletedAt, DeletedBy).
/// Follows AP.01.
/// </summary>
public interface IVKSoftDeleteAudited : IVKSoftDeletable, IVKDeletionAudited
{
}
