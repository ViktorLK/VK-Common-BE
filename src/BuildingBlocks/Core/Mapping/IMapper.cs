namespace VK.Blocks.Core.Mapping;

/// <summary>
/// Defines a contract for mapping one type to another.
/// Implement this interface to create a specific mapper for a source and destination type.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public interface IMapper<in TSource, out TDestination>
{
    /// <summary>
    /// Maps the source object to a new object of type TDestination.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns>The mapped destination object.</returns>
    TDestination Map(TSource source);
}
