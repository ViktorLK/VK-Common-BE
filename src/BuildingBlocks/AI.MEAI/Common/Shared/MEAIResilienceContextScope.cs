using System;
using System.Threading;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.MEAI.Common.Shared;

/// <summary>
/// Propagates a shared <see cref="VKResilienceContext"/> and captures per-attempt HTTP failure diagnostics
/// across delegating chat clients and <see cref="AIMEAITransportResilienceHandler"/>.
/// </summary>
// [AP.01] [ARCH-09]
internal sealed class MEAIResilienceContextScope : IDisposable
{
    private static readonly AsyncLocal<MEAIResilienceContextScope?> s_current = new();

    private readonly Lock _sync = new();
    private readonly MEAIResilienceContextScope? _parent;
    private readonly bool _restoresParentOnDispose;
    private bool _disposed;

    /// <summary>
    /// Initializes a new standalone <see cref="MEAIResilienceContextScope"/> instance without binding it to the current async flow.
    /// </summary>
    public MEAIResilienceContextScope(VKResilienceContext resilienceContext)
    {
        ResilienceContext = VKGuard.NotNull(resilienceContext);
        _restoresParentOnDispose = false;
    }

    private MEAIResilienceContextScope(VKResilienceContext resilienceContext, MEAIResilienceContextScope? parent)
    {
        ResilienceContext = VKGuard.NotNull(resilienceContext);
        _parent = parent;
        _restoresParentOnDispose = true;
    }

    /// <summary>
    /// Gets the active scope for the current asynchronous flow, if any.
    /// </summary>
    public static MEAIResilienceContextScope? Current => s_current.Value;

    /// <summary>
    /// Gets the resilience context shared across layers for the current request/attempt.
    /// </summary>
    public VKResilienceContext ResilienceContext { get; }

    /// <summary>
    /// Gets the last HTTP status code observed on the current attempt, if any.
    /// </summary>
    public int? HttpStatusCode
    {
        get
        {
            lock (_sync)
            {
                return field;
            }
        }
        private set
        {
            lock (_sync)
            {
                field = value;
            }
        }
    }

    /// <summary>
    /// Gets the last server-suggested retry-after delay observed on the current attempt, if any.
    /// </summary>
    public TimeSpan? RetryAfter
    {
        get
        {
            lock (_sync)
            {
                return field;
            }
        }
        private set
        {
            lock (_sync)
            {
                field = value;
            }
        }
    }

    /// <summary>
    /// Gets the failure disposition recorded on the current attempt.
    /// </summary>
    public VKAIFailureDisposition FailureDisposition
    {
        get
        {
            lock (_sync)
            {
                return field;
            }
        }
        private set
        {
            lock (_sync)
            {
                field = value;
            }
        }
    }

    /// <summary>
    /// Begins a new scope bound to the current asynchronous execution flow.
    /// </summary>
    public static MEAIResilienceContextScope Begin(VKResilienceContext resilienceContext)
    {
        VKGuard.NotNull(resilienceContext);
        MEAIResilienceContextScope scope = new(resilienceContext, s_current.Value);
        s_current.Value = scope;
        return scope;
    }

    /// <summary>
    /// Temporarily activates this scope on the current asynchronous flow until disposed.
    /// </summary>
    public Activation Activate()
    {
        MEAIResilienceContextScope? previous = s_current.Value;
        s_current.Value = this;
        return new Activation(this, previous);
    }

    /// <summary>
    /// Clears captured per-attempt HTTP failure diagnostics before starting a new transport attempt.
    /// </summary>
    public void ResetAttemptDiagnostics()
    {
        lock (_sync)
        {
            HttpStatusCode = null;
            RetryAfter = null;
            FailureDisposition = VKAIFailureDisposition.None;
        }
    }

    /// <summary>
    /// Records HTTP failure metadata captured at the transport boundary.
    /// </summary>
    public void RecordHttpFailure(int? statusCode, TimeSpan? retryAfter, VKAIFailureDisposition disposition)
    {
        lock (_sync)
        {
            HttpStatusCode = statusCode;
            RetryAfter = retryAfter is { } ra && ra > TimeSpan.Zero ? ra : null;
            FailureDisposition = disposition;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_restoresParentOnDispose && ReferenceEquals(s_current.Value, this))
        {
            s_current.Value = _parent;
        }
    }

    internal struct Activation : IDisposable
    {
        private readonly MEAIResilienceContextScope _scope;
        private readonly MEAIResilienceContextScope? _previous;
        private bool _active;

        internal Activation(MEAIResilienceContextScope scope, MEAIResilienceContextScope? previous)
        {
            _scope = scope;
            _previous = previous;
            _active = true;
        }

        public void Dispose()
        {
            if (!_active)
            {
                return;
            }

            _active = false;
            if (ReferenceEquals(s_current.Value, _scope))
            {
                s_current.Value = _previous;
            }
        }
    }
}
