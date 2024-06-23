using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using Web.Dashboard.Models;

namespace Web.Dashboard.Services;

public class MappingService
{
    // location where we will store the recorded remotes
    private const string PathToRemotes = "./data/remotes.json";

    private static readonly object WriteLock = new();
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
        if (!_remotes.TryRemove(remoteName, out var remote))
            return OperationResult.FailureResult("Could not find remote.");

        if (!remote.Mappings.TryRemove(mapName, out _))
            return OperationResult.FailureResult("Could not find mapped key.");

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
