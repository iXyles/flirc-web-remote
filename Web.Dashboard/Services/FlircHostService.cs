namespace Web.Dashboard.Services;

/// <summary>
/// Hosted service that initializes Flirc device on application startup
/// and ensures proper cleanup on shutdown.
/// </summary>
public class FlircHostService : IHostedService
{
    private readonly FlircDeviceManager _deviceManager;
    private readonly FlircService _service;
    private readonly MappingService _mappingService;

    public FlircHostService(
        FlircDeviceManager deviceManager,
        FlircService service,
        MappingService mappingService
    )
    {
        _deviceManager = deviceManager;
        _service = service;
        _mappingService = mappingService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _mappingService.Load();
        _service.StartProcessor();

        // Start off-thread so it does not block boot-up of the application
        // Connect device in user mode for end-users to transmit IR signals
        Task.Run(async () =>
        {
            var result = await _deviceManager.ConnectInUserMode(cancellationToken);
            if (!result.IsSuccess())
            {
                Console.WriteLine($"[FLIRC] Failed to connect: {string.Join(", ", result.ErrorMessages)}");
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _deviceManager.Disconnect();
        return Task.CompletedTask;
    }
}
