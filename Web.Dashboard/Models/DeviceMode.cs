namespace Web.Dashboard.Models;

/// <summary>
/// Represents the operational mode of the Flirc device.
/// </summary>
public enum DeviceMode
{
    /// <summary>
    /// Device is in user mode (irMode=false) for transmitting IR signals to end-users.
    /// </summary>
    UserMode,

    /// <summary>
    /// Device is in configuration mode (irMode=true) for recording new IR signals.
    /// </summary>
    ConfigMode
}
