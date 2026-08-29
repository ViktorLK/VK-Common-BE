using System;
using System.Collections.Generic;
using System.Threading;
using VK.Blocks.Core;

namespace VK.Blocks.Validation;

/// <summary>
/// Provides ambient context for validation operations, including tenant, user identity, services, and execution state.
/// </summary>
public sealed class VKValidationContext
{
    private readonly Dictionary<string, object> _items = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the cancellation token for the validation operation.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Gets the service provider, if available.
    /// </summary>
    public IServiceProvider? Services { get; init; }

    /// <summary>
    /// Gets or sets the current tenant ID associated with this validation execution.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets or sets the current user ID associated with this validation execution.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the custom contextual items collection.
    /// </summary>
    public IDictionary<string, object> Items => _items;

    public VKValidationContext(
        IServiceProvider? services = null,
        CancellationToken cancellationToken = default,
        string? tenantId = null,
        string? userId = null)
    {
        Services = services;
        CancellationToken = cancellationToken;
        TenantId = tenantId;
        UserId = userId;

        // Auto-populate from ambient accessors if available in DI container
        if (services != null)
        {
            var ambient = VKAmbientExecutionContext.Current;
            if (string.IsNullOrEmpty(TenantId) && ambient is not null && ambient.HasTenant)
            {
                TenantId = ambient.TenantId.Value.ToString();
            }

            if (string.IsNullOrEmpty(UserId) && ambient is not null && ambient.HasUser)
            {
                UserId = ambient.UserId.Value.ToString();
            }
        }
    }
}

/// <summary>
/// Strongly-typed ambient context for validation of a specific model instance.
/// </summary>
/// <typeparam name="T">The model type being validated.</typeparam>
public sealed class VKValidationContext<T> where T : class
{
    /// <summary>
    /// Gets the model instance being validated.
    /// </summary>
    public T Model { get; }

    /// <summary>
    /// Gets the underlying validation context.
    /// </summary>
    public VKValidationContext Context { get; }

    public VKValidationContext(T model, VKValidationContext? context = null)
    {
        Model = VKGuard.NotNull(model);
        Context = context ?? new VKValidationContext();
    }
}

