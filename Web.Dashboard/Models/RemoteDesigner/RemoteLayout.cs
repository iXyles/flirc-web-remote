namespace Web.Dashboard.Models.RemoteDesigner;

/// <summary>
/// Represents a complete remote layout with grid dimensions and button definitions.
/// Grid is designed to be consistent across all mobile devices.
/// </summary>
public class RemoteLayout
{
    /// <summary>
    /// Unique identifier for the layout.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name of the remote layout.
    /// </summary>
    public string Name { get; set; } = "New Remote";

    /// <summary>
    /// Number of columns in the grid (fixed for consistent mobile rendering).
    /// </summary>
    public int Columns { get; set; } = 5;

    /// <summary>
    /// Number of rows in the grid.
    /// </summary>
    public int Rows { get; set; } = 12;

    /// <summary>
    /// Background color of the remote in hex format.
    /// </summary>
    public string BackgroundColor { get; set; } = "#121212";

    /// <summary>
    /// All button definitions in the layout.
    /// </summary>
    public List<ButtonDefinition> Buttons { get; set; } = new();

    /// <summary>
    /// All recorded signals available for this remote.
    /// </summary>
    public List<Signal> Signals { get; set; } = new();

    /// <summary>
    /// Timestamp of last modification.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
