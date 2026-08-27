using System;

namespace VK.Blocks.Core;

/// <summary>
/// Marks a domain aggregate root or entity class to trigger compile-time Source Generation of:
/// 1. Encapsulated Snapshot property.
/// 2. Snapshot reconstruction constructor (with base constructor integration).
/// 3. Read-only forwarding properties from the snapshot.
/// 4. Auditing interface mutable property forwarders (<see cref="IVKFullAuditable"/>, etc.).
/// Follows AP.01, AP.03, CS.01.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VKDomainAggregateAttribute : Attribute
{
    /// <summary>
    /// Gets the snapshot state record type.
    /// </summary>
    public Type SnapshotType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKDomainAggregateAttribute"/> class.
    /// </summary>
    /// <param name="snapshotType">The immutable snapshot record type.</param>
    public VKDomainAggregateAttribute(Type snapshotType)
    {
        SnapshotType = snapshotType;
    }
}
