using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Background engine that monitors inactivity across sessions and triggers proactive callbacks.
/// Follows AP.01 (Sealed Class), AP.03 (No VK prefix), CS.03 (ConfigureAwait), and CS.06 (Deterministic clock).
/// </summary>
internal sealed class PresenceProactiveEngine : BackgroundService, IVKPresenceProactiveEngine
{
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<PresenceProactiveEngine> _logger;
    private readonly VKPresenceOptions _options;
    private readonly ConcurrentDictionary<(string SessionId, string PersonaId), DateTimeOffset> _sessions;
    private PresenceProactiveCallback? _callback;

    public PresenceProactiveEngine(
        TimeProvider timeProvider,
        IOptions<VKPresenceOptions> options,
        ILogger<PresenceProactiveEngine> logger)
    {
        _timeProvider = VKGuard.NotNull(timeProvider); // [AP.01] Boundary check
        _logger = VKGuard.NotNull(logger);
        _options = VKGuard.NotNull(options.Value);
        _sessions = new ConcurrentDictionary<(string, string), DateTimeOffset>();
    }

    /// <inheritdoc />
    public void RegisterCallback(PresenceProactiveCallback callback)
    {
        _callback = VKGuard.NotNull(callback); // [AP.01] Boundary check
    }

    /// <inheritdoc />
    public void NotifyInteraction(string sessionId, string personaId)
    {
        VKGuard.NotNullOrWhiteSpace(sessionId); // [AP.01] Boundary check
        VKGuard.NotNullOrWhiteSpace(personaId);

        var key = (sessionId, personaId);
        _sessions[key] = _timeProvider.GetUtcNow();
    }

    /// <inheritdoc />
    public async Task<VKResult> PulseAsync(
        string sessionId,
        string personaId,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(sessionId); // [AP.01] Boundary check
        VKGuard.NotNullOrWhiteSpace(personaId);

        if (_callback == null)
        {
            return VKResult.Failure(
                VKError.Failure(PresenceErrors.Proactive.CallbackNotRegistered, "Proactive callback delegate has not been registered."));
        }

        try
        {
            await _callback(sessionId, personaId, cancellationToken).ConfigureAwait(false); // [CS.03]
            NotifyInteraction(sessionId, personaId);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to force proactive pulse for Session {SessionId}.", sessionId);
            return VKResult.Failure(VKError.Failure(PresenceErrors.Proactive.PulseFailed, ex.Message));
        }
    }

    /// <summary>
    /// Executes the background hosted service execution loop.
    /// Monitors inactivity and invokes callbacks as silence thresholds are breached.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Presence Proactive Heartbeat Engine has started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Delay background loop check using the check interval option
            await Task.Delay(_options.CheckInterval, stoppingToken).ConfigureAwait(false); // [CS.03]

            if (!_options.Enabled || !_options.ProactiveEnabled || _callback == null)
            {
                continue;
            }

            var now = _timeProvider.GetUtcNow();

            foreach (var kvp in _sessions)
            {
                var key = kvp.Key;
                var lastInteraction = kvp.Value;
                var duration = now - lastInteraction;

                if (duration > _options.InactivityThreshold)
                {
                    // Reset interaction time to now to prevent immediate repeated triggers
                    _sessions[key] = now;

                    _logger.LogInformation("Inactivity threshold of {Threshold} crossed for session {SessionId}. Triggering proactive pulse callback.",
                        _options.InactivityThreshold, key.SessionId);

                    // Execute callback in the thread pool safely without blocking the hosted service loop
                    _ = Task.Run(async () =>
                    {
                        var sessionLock = VKSessionLock.GetLock(key.SessionId);

                        // Non-blocking try-acquire: if user is currently in a reactive pipeline execution, bypass/postpone proactive trigger
                        if (await sessionLock.WaitAsync(0, stoppingToken).ConfigureAwait(false))
                        {
                            try
                            {
                                await _callback(key.SessionId, key.PersonaId, stoppingToken).ConfigureAwait(false); // [CS.03]
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error occurred executing proactive callback for session {SessionId}.", key.SessionId);
                            }
                            finally
                            {
                                sessionLock.Release();
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Proactive callback bypassed for session {SessionId} because an interactive pipeline transaction is active.", key.SessionId);
                        }
                    }, stoppingToken);
                }
            }
        }

        _logger.LogInformation("Presence Proactive Heartbeat Engine has stopped.");
    }
}
