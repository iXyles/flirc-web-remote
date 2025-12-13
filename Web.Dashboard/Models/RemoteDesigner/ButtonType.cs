namespace Web.Dashboard.Models.RemoteDesigner;

/// <summary>
/// Defines the type of button for layout purposes.
/// </summary>
public enum ButtonType
{
    /// <summary>
    /// Single button occupying one grid cell.
    /// </summary>
    Single,

    /// <summary>
    /// Wide button occupying two grid cells horizontally.
    /// </summary>
    Wide,

    /// <summary>
    /// D-pad (directional pad) with up, down, left, right, and center buttons.
    /// </summary>
    DPad,

    /// <summary>
    /// Combined volume up/down buttons in a vertical arrangement.
    /// </summary>
    VolumeUpDown,

    /// <summary>
    /// Combined channel up/down buttons in a vertical arrangement.
    /// </summary>
    ChannelUpDown
}
