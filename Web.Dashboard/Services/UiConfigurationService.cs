namespace Web.Dashboard.Services;

/// <summary>
/// Manages UI configuration/edit mode state for the dashboard.
/// When edit mode is active, users can add, remove, and rename remotes and buttons.
/// </summary>
public class UiConfigurationService
{
    /// <summary>
    /// Gets whether edit mode is currently active.
    /// </summary>
    public bool IsEditModeActive { get; private set; }

    /// <summary>
    /// Raised when edit mode is toggled.
    /// </summary>
    public event EventHandler? OnEditModeChanged;

    /// <summary>
    /// Toggle edit mode on or off.
    /// </summary>
    public void ToggleEditMode()
    {
        IsEditModeActive = !IsEditModeActive;
        OnEditModeChanged?.Invoke(this, EventArgs.Empty);
    }
}
