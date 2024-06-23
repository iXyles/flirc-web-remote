using FlircWrapper;

namespace Web.Dashboard.Models;

public record MappedIr(
    string Name,
    uint ScanCode,
    RcProto Protocol,
    byte Repeat
);
