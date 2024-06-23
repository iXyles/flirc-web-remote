using System.Collections.Concurrent;

namespace Web.Dashboard.Models;

public record VirtualRemote(
    string Name,
    ConcurrentDictionary<string, MappedIr> Mappings
);
