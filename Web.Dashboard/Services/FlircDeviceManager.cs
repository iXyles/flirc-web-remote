using FlircWrapper;
using Web.Dashboard.Models;

namespace Web.Dashboard.Services;

/// <summary>
/// Manages the Flirc device connection and mode switching between user and configuration modes.
/// Ensures only one mode is active at a time for the single physical device.
/// </summary>
public class FlircDeviceManager
{
    private readonly SemaphoreSlim _modeLock = new(1, 1);

    public bool IsConnected { get; private set; }
    public DeviceMode CurrentMode { get; private set; } = DeviceMode.UserMode;

    /// <summary>
    /// Raised when device mode changes or connection state changes.
    /// </summary>
    public event EventHandler? OnModeChanged;
    public event EventHandler? OnConnectionChanged;

    /// <summary>
    /// Connect to the device in user mode (irMode=false) for transmitting IR signals.
    /// This is the default mode for end-users to operate remotes.
    /// </summary>
    public async Task<OperationResult> ConnectInUserMode(CancellationToken cancellation = default)
    {
        await _modeLock.WaitAsync(cancellation);
        try
        {
            // Close existing connection if any
            if (IsConnected)
            {
                FlircUtil.CloseDevice();
                IsConnected = false;
            }

            while (!cancellation.IsCancellationRequested)
            {
                // Try to open connection in user mode (irMode=false)
                if (FlircUtil.OpenDevice(false))
                {
                    IsConnected = true;
                    CurrentMode = DeviceMode.UserMode;
                    FlircUtil.RegisterTransmitter();

                    Console.WriteLine("[FLIRC] Device connected successfully in USER mode...");
                    OnConnectionChanged?.Invoke(this, EventArgs.Empty);
                    OnModeChanged?.Invoke(this, EventArgs.Empty);

                    return OperationResult.SuccessResult();
                }

                Console.WriteLine("[FLIRC] Checking for device...");
                var found = FlircUtil.WaitForDevice(10);
                if (!found)
                {
                    Console.WriteLine("[FLIRC] Device was not found, plug it in or re-connect it...");
                    Console.WriteLine("[FLIRC] ***NOTE: Some circumstances you may need to restart the application to connect to the device.***");
                }

                // Wait a bit before retry
                await Task.Delay(1000, cancellation);
            }

            return OperationResult.FailureResult("Connection cancelled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FLIRC] Error connecting in user mode: {ex.Message}");
            return OperationResult.FailureResult($"Failed to connect: {ex.Message}");
        }
        finally
        {
            _modeLock.Release();
        }
    }

    /// <summary>
    /// Switch device to configuration mode (irMode=true) for recording IR signals.
    /// This disconnects the device and reconnects in config mode.
    /// </summary>
    public async Task<OperationResult> SwitchToConfigMode(CancellationToken cancellation = default)
    {
        await _modeLock.WaitAsync(cancellation);
        try
        {
            Console.WriteLine("[FLIRC] Switching to CONFIG mode...");

            // Close existing connection
            if (IsConnected)
            {
                FlircUtil.CloseDevice();
                IsConnected = false;
                OnConnectionChanged?.Invoke(this, EventArgs.Empty);
            }

            // Small delay to ensure device is fully disconnected
            await Task.Delay(100, cancellation);

            // Try to open in config mode (irMode=true)
            if (FlircUtil.OpenDevice(true))
            {
                IsConnected = true;
                CurrentMode = DeviceMode.ConfigMode;

                Console.WriteLine("[FLIRC] Device connected successfully in CONFIG mode...");
                OnConnectionChanged?.Invoke(this, EventArgs.Empty);
                OnModeChanged?.Invoke(this, EventArgs.Empty);

                return OperationResult.SuccessResult();
            }

            return OperationResult.FailureResult("Failed to open device in configuration mode.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FLIRC] Error switching to config mode: {ex.Message}");
            return OperationResult.FailureResult($"Failed to switch to config mode: {ex.Message}");
        }
        finally
        {
            _modeLock.Release();
        }
    }

    /// <summary>
    /// Exit configuration mode and return to user mode.
    /// Implements automatic retry with exponential backoff on failure.
    /// </summary>
    public async Task<OperationResult> SwitchToUserMode(CancellationToken cancellation = default)
    {
        await _modeLock.WaitAsync(cancellation);
        try
        {
            Console.WriteLine("[FLIRC] Switching back to USER mode...");

            // Close existing connection
            if (IsConnected)
            {
                FlircUtil.CloseDevice();
                IsConnected = false;
                OnConnectionChanged?.Invoke(this, EventArgs.Empty);
            }

            // Small delay to ensure device is fully disconnected
            await Task.Delay(100, cancellation);

            // Retry logic with exponential backoff
            var maxRetries = 5;
            var baseDelay = 500; // Start with 500ms

            for (var attempt = 0; attempt < maxRetries; attempt++)
            {
                if (cancellation.IsCancellationRequested)
                    return OperationResult.FailureResult("Connection cancelled.");

                // Try to open in user mode (irMode=false)
                if (FlircUtil.OpenDevice(false))
                {
                    IsConnected = true;
                    CurrentMode = DeviceMode.UserMode;
                    FlircUtil.RegisterTransmitter();

                    Console.WriteLine("[FLIRC] Device reconnected successfully in USER mode...");
                    OnConnectionChanged?.Invoke(this, EventArgs.Empty);
                    OnModeChanged?.Invoke(this, EventArgs.Empty);

                    return OperationResult.SuccessResult();
                }

                // Exponential backoff: 500ms, 1s, 2s, 4s, 8s
                var delay = baseDelay * (1 << attempt);
                Console.WriteLine($"[FLIRC] Retry {attempt + 1}/{maxRetries} failed, waiting {delay}ms before next attempt...");
                await Task.Delay(delay, cancellation);
            }

            return OperationResult.FailureResult("Failed to reconnect device in user mode after multiple attempts.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FLIRC] Error switching to user mode: {ex.Message}");
            return OperationResult.FailureResult($"Failed to switch to user mode: {ex.Message}");
        }
        finally
        {
            _modeLock.Release();
        }
    }

    /// <summary>
    /// Disconnect the device completely.
    /// </summary>
    public void Disconnect()
    {
        if (IsConnected)
        {
            FlircUtil.CloseDevice();
            IsConnected = false;
            Console.WriteLine("[FLIRC] Device disconnected.");
            OnConnectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
