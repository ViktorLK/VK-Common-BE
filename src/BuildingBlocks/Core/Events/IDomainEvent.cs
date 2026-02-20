namespace VK.Blocks.Core.Events;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something meaningful that happened within the domain.
/// They are raised by aggregate roots and dispatched after persistence.
/// </summary>
public interface IDomainEvent
{
    #region Properties

    /// <summary>Gets the unique identifier for this event instance.</summary>
    Guid EventId { get; }

    /// <summary>Gets the UTC date and time when this event occurred.</summary>
    DateTimeOffset OccurredOn { get; }

    #endregion
}
