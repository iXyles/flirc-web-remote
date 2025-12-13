namespace Web.Dashboard.Models.RemoteDesigner;

/// <summary>
/// Defines a button in the remote layout with its visual properties and grid position.
/// </summary>
public class ButtonDefinition
{
    /// <summary>
    /// Unique identifier for the button.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display label for the button.
    /// </summary>
    public string Label { get; set; } = "Button";

    /// <summary>
    /// MudBlazor icon string (e.g., Icons.Material.Filled.PlayArrow).
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Button background color in hex format (e.g., "#1976d2").
    /// </summary>
    public string BackgroundColor { get; set; } = "#1976d2";

    /// <summary>
    /// Button text color in hex format.
    /// </summary>
    public string TextColor { get; set; } = "#ffffff";

    /// <summary>
    /// Type of button determining its layout behavior.
    /// </summary>
    public ButtonType Type { get; set; } = ButtonType.Single;

    /// <summary>
    /// The ID of the signal assigned to this button.
    /// </summary>
    public string? SignalId { get; set; }

    /// <summary>
    /// Grid row position (0-based).
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Grid column position (0-based).
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Number of rows this button spans.
    /// </summary>
    public int RowSpan { get; set; } = 1;

    /// <summary>
    /// Number of columns this button spans.
    /// </summary>
    public int ColumnSpan { get; set; } = 1;

    /// <summary>
    /// Faked signal ID for testing (will be replaced with actual signal later).
    /// </summary>
    public string? FakeSignalId { get; set; }
}
