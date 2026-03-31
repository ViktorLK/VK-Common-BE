namespace VK.Blocks.Mapping.Abstractions;

/// <summary>
/// Defines a contract for custom type conversion logic.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public interface ITypeConverter<in TSource, out TDestination>
{
    /// <summary>
    /// Converts the source object to the destination type.
    /// </summary>
    TDestination Convert(TSource source);
}
