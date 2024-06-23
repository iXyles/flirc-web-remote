using System.Runtime.InteropServices;

namespace FlircWrapper;

[StructLayout(LayoutKind.Sequential)]
public struct IrProt
{
    public RcProto protocol;           // Protocol
    public uint scancode;              // 32 Bit Scancode from remote
    public ulong fullcode;             // 64 bit entire code extracted from remote (don't use this)
    public uint hash;                  // 32 Bit Unique Hash
    public byte repeat;                // repeat detected = 1, 0 = no repeat
    public uint bits;                  // amount of bits collected
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)] // size from underlaying library
    public string desc;                // Used to describe our collected signal
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] // size from underlaying library
    public ushort[] buf;               // cleaned signal after decode
    public ushort len;                 // length of clean buf
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] // size from underlaying library
    public ushort[] pronto;            // pronto hex code
    public ushort pronto_len;          // pronto length
}