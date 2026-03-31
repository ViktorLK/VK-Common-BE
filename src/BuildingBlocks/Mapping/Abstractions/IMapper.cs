namespace VK.Blocks.Mapping.Abstractions;

/// <summary>
/// Defines the core contract for object-to-object mapping.
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Maps the source object to a new object of type TDestination.
    /// </summary>
    TDestination Map<TDestination>(object source);

    /// <summary>
    /// Maps the source object to the destination object.
    /// </summary>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    /// <summary>
    /// Projects the source IQueryable to a new IQueryable of type TDestination.
    /// </summary>
    IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source);
}
