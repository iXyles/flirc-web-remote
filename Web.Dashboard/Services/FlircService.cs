using System.Collections.Concurrent;
using System.Diagnostics;
using FlircWrapper;
using Web.Dashboard.Models;

namespace Web.Dashboard.Services;

public class FlircService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly BlockingCollection<MappedIr> _queue = new(new ConcurrentQueue<MappedIr>());
    private Task? _processorTask;

    public bool IsConnected { get; private set; }
    public bool IsScanning { get; private set; }
    public event EventHandler<OperationResultEventArgs>? OnTransmitResult;
    public event EventHandler? OnConnectionChanged;

    public void ConnectDevice(CancellationToken cancellation)
    {
        while (true)
        {
            if (cancellation.IsCancellationRequested) return;

            // try open connection to device
            if (FlircUtil.OpenDevice(false))
            {
                IsConnected = true;
                FlircUtil.RegisterTransmitter();
                OnConnectionChanged?.Invoke(this, EventArgs.Empty);

                Console.WriteLine("[FLIRC] device connected successfully...");
            }
            else
            {
                Console.WriteLine("[FLIRC] Checking for device...");
                var found = FlircUtil.WaitForDevice(10);
                if (!found)
                {
                    Console.WriteLine("[FLIRC] Device was not found, plug it in or re-connect it...");
                    // Guessing there is some sort of issue with the "timout" function that causes the failing "wait" to not work properly
                    Console.WriteLine("[FLIRC] ***NOTE: Some circumstances you may need to restart the application to connect to the device.***");
                }

                continue;
            }

            // TODO : How to handle "re-connects" of the device? and how do we detect disconnects (?)
            break;
        }
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
        if (!IsConnected)
            return OperationResult.FailureResult("Cannot queue button, the device is not connected.");

        if (IsScanning)
            return OperationResult.FailureResult("Cannot queue button, someone is currently adding a new key.");

        _queue.Add(mapped);
        return OperationResult.SuccessResult();
    }
}

public class OperationResultEventArgs : EventArgs
{
    public string Key { get; }
    public OperationResult OperationResult { get; }

    public OperationResultEventArgs(OperationResult operationResult, string key)
    {
        OperationResult = operationResult;
        Key = key;
    }
}
