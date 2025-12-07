using System.Collections.Concurrent;
using System.Diagnostics;
using FlircWrapper;
using Web.Dashboard.Models;

namespace Web.Dashboard.Services;

/// <summary>
/// Handles IR signal transmission queue processing for end-users.
/// Depends on FlircDeviceManager for device connection state and mode.
/// </summary>
public class FlircService
{
    private readonly FlircDeviceManager _deviceManager;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly BlockingCollection<MappedIr> _queue = new(new ConcurrentQueue<MappedIr>());
    private Task? _processorTask;

    public bool IsScanning { get; private set; }
    public event EventHandler<OperationResultEventArgs>? OnTransmitResult;

    public FlircService(FlircDeviceManager deviceManager)
    {
        _deviceManager = deviceManager;
    }

    public void StartProcessor() =>
        _processorTask ??= Task.Run(ProcessQueue);

    private async Task ProcessQueue()
    {
        foreach (var mapped in _queue.GetConsumingEnumerable())
        {
            await _semaphore.WaitAsync();
            try
            {
                Debug.WriteLine($"[PROCESS SEND] Transmitting packet for {mapped.Name}...");
                var result = FlircUtil.SendPacket(mapped.Buffer);
                Debug.WriteLine($"[PROCESS SEND] Transmission completed for {mapped.Name}...");
                OnTransmitResult?.Invoke(
                    this,
                    new OperationResultEventArgs(
                        result == 0
                            ? OperationResult.SuccessResult()
                            : OperationResult.FailureResult($"Failed to transmit... error code: {result}"
                        ),
                        mapped.Name
                    )
                );
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public async Task<OperationResult<IrProt>> StartPoll(CancellationToken token)
    {
        IsScanning = true;
        await _semaphore.WaitAsync(token);
        try
        {
            return await Task.Run(() => PollInNewThread(token), token);
        }
        finally
        {
            IsScanning = false;
            _semaphore.Release();
        }
    }

    private Task<OperationResult<IrProt>> PollInNewThread(CancellationToken token)
    {
        var protos = FlircUtil.ListenToPoll(token);
        foreach (var proto in protos)
        {
            // return first "proto/packet" - this is from testing of my devices that has worked...
            // if we need to send more than a single packet for something, then this gotta be changed, along "mapped" object
            return Task.FromResult(OperationResults.Success(proto));
        }
        return Task.FromResult(OperationResult<IrProt>.FailureResult("Returned no polling result."));
    }

    public OperationResult QueueTransmit(MappedIr mapped)
    {
        if (!_deviceManager.IsConnected)
            return OperationResult.FailureResult("Cannot queue button, the device is not connected.");

        if (_deviceManager.CurrentMode != DeviceMode.UserMode)
            return OperationResult.FailureResult("Cannot queue button, the device is in configuration mode.");

        if (IsScanning)
            return OperationResult.FailureResult("Cannot queue button, someone is currently adding a new key.");

        _queue.Add(mapped);
        return OperationResult.SuccessResult();
    }

    /// <summary>
    /// Cancel all pending transmissions in the queue.
    /// Called when entering configuration mode.
    /// </summary>
    public void CancelPendingTransmissions()
    {
        var count = 0;
        while (_queue.TryTake(out _))
        {
            count++;
        }

        if (count > 0)
        {
            Console.WriteLine($"[FLIRC] Cancelled {count} pending transmission(s).");
        }
    }
}
