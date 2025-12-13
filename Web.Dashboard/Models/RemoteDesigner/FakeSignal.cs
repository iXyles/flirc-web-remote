namespace Web.Dashboard.Models.RemoteDesigner;

/// <summary>
/// Fake signal for testing the designer before integrating real IR signals.
/// </summary>
public class FakeSignal
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Signal";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = MudBlazor.Icons.Material.Filled.Circle;

    /// <summary>
    /// Predefined fake signals for testing.
    /// </summary>
    public static List<FakeSignal> GetDefaultSignals() =>
    [
        new() { Name = "Power", Description = "Power On/Off", Icon = MudBlazor.Icons.Material.Filled.PowerSettingsNew },
        new() { Name = "Play", Description = "Play", Icon = MudBlazor.Icons.Material.Filled.PlayArrow },
        new() { Name = "Pause", Description = "Pause", Icon = MudBlazor.Icons.Material.Filled.Pause },
        new() { Name = "Stop", Description = "Stop", Icon = MudBlazor.Icons.Material.Filled.Stop },
        new() { Name = "Volume Up", Description = "Increase Volume", Icon = MudBlazor.Icons.Material.Filled.VolumeUp },
        new() { Name = "Volume Down", Description = "Decrease Volume", Icon = MudBlazor.Icons.Material.Filled.VolumeDown },
        new() { Name = "Mute", Description = "Mute", Icon = MudBlazor.Icons.Material.Filled.VolumeOff },
        new() { Name = "Menu", Description = "Menu", Icon = MudBlazor.Icons.Material.Filled.Menu },
        new() { Name = "Back", Description = "Back", Icon = MudBlazor.Icons.Material.Filled.ArrowBack },
        new() { Name = "Home", Description = "Home", Icon = MudBlazor.Icons.Material.Filled.Home },
        new() { Name = "OK", Description = "OK/Select", Icon = MudBlazor.Icons.Material.Filled.CheckCircle }
    ];
}
