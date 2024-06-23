using System.Runtime.InteropServices;

namespace FlircWrapper;

[StructLayout(LayoutKind.Sequential)]
public struct IrPacket
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public ushort[] buf;               // Buffer
    public ushort len;                 // Length of buffer
    public ushort elapsed;             // Elapsed time
}