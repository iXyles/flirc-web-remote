using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace FlircWrapper;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class IrLibraryWrapper
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr ir_lib_version_delegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate RcProto ir_decode_packet_delegate(ref IrPacket packet, ref IrProt protocol);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr ir_tx_delegate(RcProto protocol, uint scancode, int repeat);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr ir_register_tx_delegate(IrRegisterTxCallback transmitFunction);

    public static ir_lib_version_delegate ir_lib_version = null!;
    public static ir_decode_packet_delegate ir_decode_packet = null!;
    public static ir_tx_delegate ir_tx = null!;
    public static ir_register_tx_delegate ir_register_tx = null!;

    public delegate int IrRegisterTxCallback(IntPtr buf, ushort len, ushort ik, byte rep);

    static IrLibraryWrapper()
    {
        try
        {
            var dllName = GetLibraryFileBasedOnPlatform();
            var module = LibResolver.LoadLib(dllName);

            LibResolver.AssignFunctionPointer(module, nameof(ir_lib_version), out ir_lib_version);
            LibResolver.AssignFunctionPointer(module, nameof(ir_decode_packet), out ir_decode_packet);
            LibResolver.AssignFunctionPointer(module, nameof(ir_tx), out ir_tx);
            LibResolver.AssignFunctionPointer(module, nameof(ir_register_tx), out ir_register_tx);
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
            Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "libs/win-x64/libir.dll",

            // LINUX
            //Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "libs/linux-x86_64/libir.so.3.27.15",
            //Architecture.X86 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "libs/linux-x86_64/libir.so.3.27.15",
            Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "libs/linux-arm64/libir.so.3.27.15",
            //Architecture.Arm when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "libs/linux-arm/libir.so.3.27.15",

            // MAC
            //Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "libs/macos-x64/libir.3.27.15.dylib",
            Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "libs/macos-arm64/libir.3.27.15.dylib",

            _ => throw new PlatformNotSupportedException("Unsupported platform")
        };
}
