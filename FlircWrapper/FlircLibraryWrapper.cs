using System.Runtime.InteropServices;

namespace FlircWrapper;

public static class FlircLibraryWrapper
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int fl_open_device_alt_delegate(ushort vendorId, string deviceId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int fl_wait_for_device_delegate(ushort vendorId, string deviceId);

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

    public static fl_open_device_alt_delegate fl_open_device_alt;
    public static fl_wait_for_device_delegate fl_wait_for_device;
    public static fl_lib_version_delegate fl_lib_version;
    public static fl_ir_packet_poll_delegate fl_ir_packet_poll;
    public static fl_close_device_delegate fl_close_device;
    public static fl_dev_flush_delegate fl_dev_flush;
    public static fl_transmit_raw_delegate fl_transmit_raw;

    static FlircLibraryWrapper()
    {
        try
        {
            var dllName = GetLibraryFileBasedOnPlatform();
            var module = LibResolver.LoadLib(dllName);

            LibResolver.AssignFunctionPointer(module, nameof(fl_open_device_alt), out fl_open_device_alt);
            LibResolver.AssignFunctionPointer(module, nameof(fl_wait_for_device), out fl_wait_for_device);
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
