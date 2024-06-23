using System.Runtime.InteropServices;

namespace FlircWrapper;

public static class IrSdkWrapper
{
    private const string DllName = "libs/macos-arm/libir.3.27.15.dylib";

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ir_lib_version();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern RcProto ir_decode_packet(ref IrPacket packet, ref IrProt protocol);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ir_tx(RcProto protocol, uint scancode, int repeat);

    [DllImport(DllName)]
    public static extern IntPtr ir_register_tx(IrRegisterTxCallback transmitFunction);

    public delegate int IrRegisterTxCallback(IntPtr buf, ushort len, ushort ik, byte rep);
}
