namespace Web.Dashboard.Models.RemoteDesigner;

/// <summary>
/// Represents a recorded IR signal.
/// </summary>
public class Signal
{
    /// <summary>
    /// Unique identifier for the signal.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name of the signal (e.g., "Power", "Volume Up").
    /// </summary>
    public string Name { get; set; } = "New Signal";

    /// <summary>
    /// The raw IR buffer data.
    /// </summary>
    public short[] Buffer { get; set; } = [];
    
    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; set; }
}
