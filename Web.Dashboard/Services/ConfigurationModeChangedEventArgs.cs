namespace Web.Dashboard.Services;

/// <summary>
/// Event args for configuration mode changes.
/// </summary>
public class ConfigurationModeChangedEventArgs : EventArgs
{
    public bool IsActive { get; }

    public ConfigurationModeChangedEventArgs(bool isActive)
    {
        IsActive = isActive;
    }
}
