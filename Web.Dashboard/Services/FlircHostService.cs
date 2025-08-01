using FlircWrapper;

namespace Web.Dashboard.Services;

public class FlircHostService : IHostedService
{
    private readonly FlircService _service;
    private readonly MappingService _mappingService;

    public FlircHostService(
        FlircService service,
        MappingService mappingService
    )
    {
        _service = service;
        _mappingService = mappingService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _mappingService.Load();
        _service.StartProcessor();
        // start off-thread so it does not block boot-up of the application
        Task.Run(() => _service.ConnectDevice(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        FlircUtil.CloseDevice();
        return Task.CompletedTask;
    }
}
