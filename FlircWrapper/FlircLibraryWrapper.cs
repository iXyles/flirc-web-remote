using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming

namespace FlircWrapper;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class FlircLibraryWrapper
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int fl_open_device_delegate(ushort vendorId, string deviceId);

    // alt open must be used to enable "IR Raw endpoint" - example when we want to record an incoming packet
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int fl_open_device_alt_delegate(ushort vendorId, string deviceId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int fl_wait_for_device_delegate(ushort vendorId, string deviceId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int fl_wait_for_device_timeout_delegate(ushort vendorId, string deviceId, int timeout);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr fl_lib_version_delegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int fl_ir_packet_poll_delegate(ref IrPacket packet);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void fl_close_device_delegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void fl_dev_flush_delegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int fl_transmit_raw_delegate(IntPtr buf, ushort len, ushort ik, byte repeat);

    public static fl_open_device_delegate fl_open_device = null!;
    public static fl_open_device_alt_delegate fl_open_device_alt = null!;
    public static fl_wait_for_device_delegate fl_wait_for_device = null!;
    public static fl_wait_for_device_timeout_delegate fl_wait_for_device_timeout = null!;
    public static fl_lib_version_delegate fl_lib_version = null!;
    public static fl_ir_packet_poll_delegate fl_ir_packet_poll = null!;
    public static fl_close_device_delegate fl_close_device = null!;
    public static fl_dev_flush_delegate fl_dev_flush = null!;
    public static fl_transmit_raw_delegate fl_transmit_raw = null!;

    static FlircLibraryWrapper()
    {
        try
        {
            var dllName = GetLibraryFileBasedOnPlatform();
            var module = LibResolver.LoadLib(dllName);

            LibResolver.AssignFunctionPointer(module, nameof(fl_open_device), out fl_open_device);
            LibResolver.AssignFunctionPointer(module, nameof(fl_open_device_alt), out fl_open_device_alt);
            LibResolver.AssignFunctionPointer(module, nameof(fl_wait_for_device), out fl_wait_for_device);
            LibResolver.AssignFunctionPointer(module, nameof(fl_wait_for_device_timeout), out fl_wait_for_device_timeout);
            LibResolver.AssignFunctionPointer(module, nameof(fl_lib_version), out fl_lib_version);
            LibResolver.AssignFunctionPointer(module, nameof(fl_ir_packet_poll), out fl_ir_packet_poll);
            LibResolver.AssignFunctionPointer(module, nameof(fl_close_device), out fl_close_device);
            LibResolver.AssignFunctionPointer(module, nameof(fl_dev_flush), out fl_dev_flush);
            LibResolver.AssignFunctionPointer(module, nameof(fl_transmit_raw), out fl_transmit_raw);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }

    static string GetLibraryFileBasedOnPlatform() =>
        RuntimeInformation.ProcessArchitecture switch
        {
            // WINDOWS
            Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "libs/win-x64/libflirc.dll",

            // LINUX
            //Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "libs/linux-x86_64/libflirc.so.3.27.15",
            //Architecture.X86 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "libs/linux-x86_64/libflirc.so.3.27.15",
            Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "libs/linux-arm64/libflirc.so.3.27.15",
            //Architecture.Arm when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "libs/linux-arm/libflirc.so.3.27.15",

            // MAC
            //Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "libs/macos-x64/libflirc.3.27.15.dylib",
            Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "libs/macos-arm64/libflirc.3.27.15.dylib",

            _ => throw new PlatformNotSupportedException("Unsupported platform")
        };
}
