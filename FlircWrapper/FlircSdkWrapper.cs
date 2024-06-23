using System.Runtime.InteropServices;

namespace FlircWrapper;

public static class FlircSdkWrapper
{
    // Dll for Mac ARM (m1>)
    private const string DllName = "libs/macos-arm/libflirc.3.27.15.dylib";

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fl_open_device_alt(ushort vendorId, string deviceId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fl_wait_for_device(ushort vendorId, string deviceId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr fl_lib_version();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fl_ir_packet_poll(ref IrPacket packet);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fl_close_device();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fl_dev_flush();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fl_transmit_raw(IntPtr buf, ushort len, ushort ik, byte repeat);
}
