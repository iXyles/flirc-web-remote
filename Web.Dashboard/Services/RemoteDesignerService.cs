using System.Text.Json;
using Web.Dashboard.Models.RemoteDesigner;

namespace Web.Dashboard.Services;

/// <summary>
/// Service for managing remote designer layouts and signals with persistence.
/// </summary>
public class RemoteDesignerService
{
    private const string LayoutFilePath = "./data/layout.json";
    private RemoteLayout _currentLayout = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public event EventHandler? OnLayoutChanged;

    public RemoteDesignerService()
    {
        LoadLayout();
    }

    public RemoteLayout GetCurrentLayout() => _currentLayout;

    public void AddButton(ButtonDefinition button)
    {
        _currentLayout.Buttons.Add(button);
        SaveLayout();
        NotifyChanged();
    }

    public void RemoveButton(string buttonId)
    {
        _currentLayout.Buttons.RemoveAll(b => b.Id == buttonId);
        SaveLayout();
        NotifyChanged();
    }

    public void UpdateButton(ButtonDefinition button)
    {
        var index = _currentLayout.Buttons.FindIndex(b => b.Id == button.Id);
        if (index != -1)
        {
            _currentLayout.Buttons[index] = button;
            SaveLayout();
            NotifyChanged();
        }
    }

    public void ClearLayout()
    {
        _currentLayout.Buttons.Clear();
        _currentLayout.Signals.Clear();
        SaveLayout();
        NotifyChanged();
    }

    // --- Signal Management ---

    public void AddSignal(Signal signal)
    {
        _currentLayout.Signals.Add(signal);
        SaveLayout();
        NotifyChanged();
    }

    public void RemoveSignal(string signalId)
    {
        _currentLayout.Signals.RemoveAll(s => s.Id == signalId);
        // Also clear references from buttons
        foreach (var btn in _currentLayout.Buttons.Where(b => b.SignalId == signalId))
        {
            btn.SignalId = null;
        }
        SaveLayout();
        NotifyChanged();
    }

    public void UpdateSignal(Signal signal)
    {
        var index = _currentLayout.Signals.FindIndex(s => s.Id == signal.Id);
        if (index != -1)
        {
            _currentLayout.Signals[index] = signal;
            SaveLayout();
            NotifyChanged();
        }
    }

    public Signal? GetSignal(string signalId)
    {
        return _currentLayout.Signals.FirstOrDefault(s => s.Id == signalId);
    }

    public List<Signal> GetSignals()
    {
        return _currentLayout.Signals;
    }

    // --- Persistence ---

    private void LoadLayout()
    {
        try
        {
            if (File.Exists(LayoutFilePath))
            {
                var json = File.ReadAllText(LayoutFilePath);
                var layout = JsonSerializer.Deserialize<RemoteLayout>(json);
                if (layout != null)
                {
                    _currentLayout = layout;
                }
            }
            else
            {
                // Ensure directory exists
                var dir = Path.GetDirectoryName(LayoutFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading layout: {ex.Message}");
            // Fallback to new layout
            _currentLayout = new RemoteLayout();
        }
    }

    private void SaveLayout()
    {
        try
        {
            _currentLayout.LastModified = DateTime.UtcNow;
            var json = JsonSerializer.Serialize(_currentLayout, _jsonOptions);
            
            var dir = Path.GetDirectoryName(LayoutFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(LayoutFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving layout: {ex.Message}");
        }
    }

    private void NotifyChanged()
    {
        OnLayoutChanged?.Invoke(this, EventArgs.Empty);
    }
}
