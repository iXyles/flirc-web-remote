using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Web.Dashboard.Services;

/// <summary>
/// Handles Blazor circuit lifecycle events to automatically clean up configuration mode
/// when an admin's connection is lost (e.g., browser closed, network disconnected).
/// </summary>
public class FlircCircuitHandler : CircuitHandler
{
    private readonly ConfigurationService _configurationService;
    private readonly CircuitAccessor _circuitAccessor;

    public FlircCircuitHandler(ConfigurationService configurationService, CircuitAccessor circuitAccessor)
    {
        _configurationService = configurationService;
        _circuitAccessor = circuitAccessor;
    }

    /// <summary>
    /// Called when a circuit is opened (user connects).
    /// Sets the circuit ID in the scoped CircuitAccessor so components can access it.
    /// </summary>
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _circuitAccessor.CircuitId = circuit.Id;
        Console.WriteLine($"[CIRCUIT] Circuit opened: {circuit.Id}");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a circuit is closed (user disconnects).
    /// Automatically exits configuration mode if this circuit was in config mode.
    /// </summary>
    public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[CIRCUIT] Circuit closed: {circuit.Id}");

        if (_circuitAccessor.CircuitId is not null)
        {
            // Force exit configuration mode for this circuit
            await _configurationService.ForceExitConfigMode(_circuitAccessor.CircuitId);
        }
    }

    /// <summary>
    /// Called when a connection error occurs.
    /// Treat as a disconnect and clean up configuration mode.
    /// </summary>
    public override async Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[CIRCUIT] Connection down: {circuit.Id}");

        if (_circuitAccessor.CircuitId is not null)
        {
            // Force exit configuration mode for this circuit
            await _configurationService.ForceExitConfigMode(_circuitAccessor.CircuitId);
        }
    }
}

