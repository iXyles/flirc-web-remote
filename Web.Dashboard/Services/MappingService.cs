using System.Collections.Concurrent;
using System.Text.Json;
using Web.Dashboard.Models;

namespace Web.Dashboard.Services;

public class MappingService
{
    // location where we will store the recorded remotes
    private const string PathToRemotes = "./data/remotes.json";

    private static readonly Lock WriteLock = new();
    private ConcurrentDictionary<string, VirtualRemote> _remotes = new();
    private readonly JsonSerializerOptions _options = JsonSerializerOptions.Default;

    public event EventHandler? OnChange;

    public IEnumerable<VirtualRemote> GetVirtualRemotes()
    {
        foreach (var remote in _remotes)
            yield return remote.Value;
    }

    public IEnumerable<MappedIr> GetRemoteMappings(string? remoteName)
    {
        if (string.IsNullOrWhiteSpace(remoteName))
            yield break;

        if (_remotes.TryGetValue(remoteName, out var remote))
        {
            foreach (var mapping in remote.Mappings)
                yield return mapping.Value;
        }
    }

    /// <summary>
    /// Load saved "virtual remotes" and their connected "mappings".
    /// </summary>
    public void Load()
    {
        if (File.Exists(PathToRemotes))
        {
            using var file = File.OpenRead(PathToRemotes);
            _remotes = JsonSerializer.Deserialize<ConcurrentDictionary<string, VirtualRemote>>(file, _options) ??
                       new ConcurrentDictionary<string, VirtualRemote>();
        }
    }

    /// <summary>
    /// Add & store a new virtual remote.
    /// </summary>
    public OperationResult AddRemote(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.FailureResult("A remote must have a name.");

        var remote = new VirtualRemote(name, new ConcurrentDictionary<string, MappedIr>());
        if (!_remotes.TryAdd(name, remote))
            return OperationResult.FailureResult("Failed to add remote, possible duplicate name.");

        SaveRemote();
        return OperationResult.SuccessResult();
    }

    /// <summary>
    /// Remove existing virtual remote & IR mappings connected to "that" virtual remote.
    /// </summary>
    public OperationResult RemoveRemote(string name)
    {
        if (!_remotes.TryRemove(name, out _))
            return OperationResult.FailureResult("Could not find remote.");

        SaveRemote();
        return OperationResult.SuccessResult();
    }

    /// <summary>
    /// Add mapping of an IR connection to a virtual remote.
    /// </summary>
    public OperationResult AddMapping(string remoteName, MappedIr mapped)
    {
        if (string.IsNullOrWhiteSpace(mapped.Name))
            return OperationResult.FailureResult("Must map with a key/name");

        if (!_remotes.TryGetValue(remoteName, out var remote))
            return OperationResult.FailureResult("Could not find remote.");

        if (remote.Mappings.TryGetValue(mapped.Name, out _))
            return OperationResult.FailureResult("A mapping to this key/name already exists.");

        if (!remote.Mappings.TryAdd(mapped.Name, mapped))
            return OperationResult.FailureResult("Something went wrong.");

        SaveRemote();
        return OperationResult.SuccessResult();
    }

    /// <summary>
    /// Remove a registered mapping of an IR connection connected a virtual remote.
    /// </summary>
    public OperationResult RemoveRemoteMapping(string remoteName, string mapName)
    {
        if (!_remotes.TryGetValue(remoteName, out var remote))
            return OperationResult.FailureResult("Could not find remote.");

        if (!remote.Mappings.TryRemove(mapName, out _))
            return OperationResult.FailureResult("Could not find button.");

        SaveRemote();
        return OperationResult.SuccessResult();
    }

    /// <summary>
    /// Rename an existing remote. Validates that the new name is unique.
    /// </summary>
    public OperationResult RenameRemote(string oldName, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return OperationResult.FailureResult("Remote name cannot be empty.");

        if (oldName == newName)
            return OperationResult.SuccessResult(); // No change needed

        if (!_remotes.TryGetValue(oldName, out var remote))
            return OperationResult.FailureResult("Could not find remote.");

        // Check if new name already exists
        if (_remotes.ContainsKey(newName))
            return OperationResult.FailureResult("A remote with this name already exists.");

        // Remove old entry and add with new name
        if (!_remotes.TryRemove(oldName, out _))
            return OperationResult.FailureResult("Failed to remove old remote entry.");

        var renamedRemote = remote with { Name = newName };
        if (!_remotes.TryAdd(newName, renamedRemote))
        {
            // Rollback: add the old one back
            _remotes.TryAdd(oldName, remote);
            return OperationResult.FailureResult("Failed to add renamed remote.");
        }

        SaveRemote();
        return OperationResult.SuccessResult();
    }

    /// <summary>
    /// Rename a button/mapping within a remote. Validates that the new name is unique within the remote.
    /// </summary>
    public OperationResult RenameMapping(string remoteName, string oldButtonName, string newButtonName)
    {
        if (string.IsNullOrWhiteSpace(newButtonName))
            return OperationResult.FailureResult("Button name cannot be empty.");

        if (oldButtonName == newButtonName)
            return OperationResult.SuccessResult(); // No change needed

        if (!_remotes.TryGetValue(remoteName, out var remote))
            return OperationResult.FailureResult("Could not find remote.");

        if (!remote.Mappings.TryGetValue(oldButtonName, out var mapping))
            return OperationResult.FailureResult("Could not find button.");

        // Check if new name already exists in this remote
        if (remote.Mappings.ContainsKey(newButtonName))
            return OperationResult.FailureResult("A button with this name already exists in this remote.");

        // Remove old entry and add with new name
        if (!remote.Mappings.TryRemove(oldButtonName, out _))
            return OperationResult.FailureResult("Failed to remove old button entry.");

        var renamedMapping = mapping with { Name = newButtonName };
        if (!remote.Mappings.TryAdd(newButtonName, renamedMapping))
        {
            // Rollback: add the old one back
            remote.Mappings.TryAdd(oldButtonName, mapping);
            return OperationResult.FailureResult("Failed to add renamed button.");
        }

        SaveRemote();
        return OperationResult.SuccessResult();
    }

    /// <summary>
    /// Save the "virtual remotes" with their "mappings" so we can restart app without losing data.
    /// </summary>
    private void SaveRemote()
    {
        lock (WriteLock)
        {
            // re-save all "remotes"
            EnsureDirectoryExists(PathToRemotes);
            using var file = File.OpenWrite(PathToRemotes);
            JsonSerializer.Serialize(file, _remotes, _options);
            OnChange?.Invoke(this, EventArgs.Empty); // TODO : Might have to change so this is more "granular"
        }
    }

    private static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
