using FlircWrapper;

namespace Web.Dashboard.Services;

public class FlircHostService : IHostedService
{
    private readonly FlircServiceHandler _serviceHandler;
    private readonly FlircService _flircService;
    private readonly MappingService _mappingService;

    public FlircHostService(
        FlircServiceHandler serviceHandler,
        FlircService flircService,
        MappingService mappingService
    )
    {
        _serviceHandler = serviceHandler;
        _flircService = flircService;
        _mappingService = mappingService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _mappingService.Load();
        _serviceHandler.StartProcessor();
        // start off-thread so it does not block boot-up of the application
        Task.Run(() => _serviceHandler.ConnectDevice(), cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _flircService.CloseConnection();
        return Task.CompletedTask;
    }
}
