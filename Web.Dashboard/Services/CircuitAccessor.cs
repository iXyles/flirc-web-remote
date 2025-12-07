namespace Web.Dashboard.Services;

/// <summary>
/// Provides access to the current Blazor circuit ID for the active connection.
/// This is scoped per-circuit to ensure each user gets their own circuit ID.
/// </summary>
public class CircuitAccessor
{
    public string? CircuitId { get; set; }
}
