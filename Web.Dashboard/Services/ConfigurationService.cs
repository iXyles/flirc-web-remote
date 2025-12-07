using Web.Dashboard.Models;

namespace Web.Dashboard.Services;

/// <summary>
/// Manages configuration mode sessions for the Flirc device.
/// Ensures only one admin can configure the device at a time, tied to their Blazor circuit.
/// </summary>
public class ConfigurationService
{
    private readonly FlircDeviceManager _deviceManager;
    private readonly SemaphoreSlim _sessionLock = new(1, 1);
    private string? _activeCircuitId;

    /// <summary>
    /// Raised when configuration mode is entered or exited.
    /// </summary>
    public event EventHandler<ConfigurationModeChangedEventArgs>? OnConfigurationModeChanged;

    public ConfigurationService(FlircDeviceManager deviceManager)
    {
        _deviceManager = deviceManager;
    }

    /// <summary>
    /// Gets whether configuration mode is currently active.
    /// </summary>
    public bool IsConfigurationModeActive => _activeCircuitId is not null;

    /// <summary>
    /// Attempt to enter configuration mode for the specified circuit.
    /// Only one circuit can be in configuration mode at a time.
    /// </summary>
    public async Task<OperationResult> TryEnterConfigMode(string circuitId, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(circuitId))
            return OperationResult.FailureResult("Circuit ID is required.");

        await _sessionLock.WaitAsync(cancellation);
        try
        {
            // Check if another session is already active
            if (_activeCircuitId is not null)
            {
                if (_activeCircuitId == circuitId)
                {
                    // Same circuit trying to re-enter (shouldn't happen normally)
                    return OperationResult.SuccessResult();
                }

                return OperationResult.FailureResult("Configuration mode is currently being used by another user. Please wait.");
            }

            // Switch device to configuration mode
            var result = await _deviceManager.SwitchToConfigMode(cancellation);
            if (!result.IsSuccess())
            {
                return result;
            }

            // Mark this circuit as the active configuration session
            _activeCircuitId = circuitId;
            Console.WriteLine($"[CONFIG] Circuit {circuitId} entered configuration mode.");

            OnConfigurationModeChanged?.Invoke(this, new ConfigurationModeChangedEventArgs(true));
            return OperationResult.SuccessResult();
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    /// <summary>
    /// Exit configuration mode for the specified circuit.
    /// If the circuit matches the active session, the device will return to user mode.
    /// </summary>
    public async Task<OperationResult> ExitConfigMode(string circuitId, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(circuitId))
            return OperationResult.FailureResult("Circuit ID is required.");

        await _sessionLock.WaitAsync(cancellation);
        try
        {
            // Only allow exit if this circuit owns the session
            if (_activeCircuitId is null)
            {
                // No active session, nothing to do
                return OperationResult.SuccessResult();
            }

            if (_activeCircuitId != circuitId)
            {
                // Different circuit trying to exit
                return OperationResult.FailureResult("This circuit does not own the active configuration session.");
            }

            // Clear the active session
            _activeCircuitId = null;
            Console.WriteLine($"[CONFIG] Circuit {circuitId} exited configuration mode.");

            // Switch device back to user mode
            var result = await _deviceManager.SwitchToUserMode(cancellation);

            OnConfigurationModeChanged?.Invoke(this, new ConfigurationModeChangedEventArgs(false));

            return result;
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    /// <summary>
    /// Force exit configuration mode (used by circuit handler on disconnect).
    /// </summary>
    internal async Task ForceExitConfigMode(string circuitId)
    {
        try
        {
            await ExitConfigMode(circuitId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CONFIG] Error force-exiting config mode for circuit {circuitId}: {ex.Message}");
        }
    }
}
