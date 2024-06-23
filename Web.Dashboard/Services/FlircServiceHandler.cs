using System.Collections.Concurrent;
using FlircWrapper;
using Web.Dashboard.Models;

namespace Web.Dashboard.Services;

public class FlircServiceHandler
{
    private readonly FlircService _flircService;

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly BlockingCollection<MappedIr> _queue = new(new ConcurrentQueue<MappedIr>());

    private Task? _processorTask;

    public FlircServiceHandler(FlircService flircService)
    {
        _flircService = flircService;
    }

    public bool IsConnected { get; private set; }
    public bool IsScanning { get; private set; }
    public event EventHandler<OperationResultEventArgs>? OnTransmitResult;

    public void ConnectDevice()
    {
        // try open connection to device
        if (_flircService.OpenConnection())
        {
            IsConnected = true;
            _flircService.RegisterTransmitter();
        }
        // if failed, maybe disconnected, then wait till it is connected
        else if (_flircService.WaitForDevice())
        {
            IsConnected = true;
            _flircService.RegisterTransmitter();
        }
        // TODO : How to handle "re-connects" of the device? and how do we detect disconnects (?)
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
                var result = _flircService.SendPacket(new IrProt
                {
                    protocol = mapped.Protocol,
                    scancode = mapped.ScanCode,
                    repeat = mapped.Repeat
                });
                OnTransmitResult?.Invoke(
                    this,
                    new OperationResultEventArgs(
                        result == 0
                            ? OperationResult.SuccessResult()
                            : OperationResult.FailureResult($"Failed to transmit... error code: {result}"),
                        mapped.Name
                    ));
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
        var protos = _flircService.ListenToPoll(token);
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
