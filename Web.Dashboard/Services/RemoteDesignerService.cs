using Web.Dashboard.Models.RemoteDesigner;

namespace Web.Dashboard.Services;

/// <summary>
/// Service for managing remote designer layouts in memory.
/// Persistence is not implemented - layouts are temporary for testing.
/// </summary>
public class RemoteDesignerService
{
    private RemoteLayout _currentLayout = new();
    private readonly List<FakeSignal> _fakeSignals = FakeSignal.GetDefaultSignals();

    public event EventHandler? OnLayoutChanged;

    public RemoteLayout GetCurrentLayout() => _currentLayout;

    public List<FakeSignal> GetFakeSignals() => _fakeSignals;

    public void AddButton(ButtonDefinition button)
    {
        _currentLayout.Buttons.Add(button);
        _currentLayout.LastModified = DateTime.UtcNow;
        OnLayoutChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveButton(string buttonId)
    {
        _currentLayout.Buttons.RemoveAll(b => b.Id == buttonId);
        _currentLayout.LastModified = DateTime.UtcNow;
        OnLayoutChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateButton(ButtonDefinition button)
    {
        var existing = _currentLayout.Buttons.FirstOrDefault(b => b.Id == button.Id);
        if (existing is not null)
        {
            _currentLayout.Buttons.Remove(existing);
            _currentLayout.Buttons.Add(button);
            _currentLayout.LastModified = DateTime.UtcNow;
            OnLayoutChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Clear the current layout.
    /// </summary>
    public void ClearLayout()
    {
        _currentLayout = new RemoteLayout();
        OnLayoutChanged?.Invoke(this, EventArgs.Empty);
    }
}
