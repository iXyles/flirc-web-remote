using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace FlircWrapper;

/// <summary>
/// Basic service to utilize underlaying libraries of Flirc
/// </summary>
public static class FlircUtil
{
    const ushort VendorId = 0x20A0; // Flirc vendor ID
    const string DeviceId = "flirc.tv"; // Flirc device ID

    public static string? FetchFlircLibraryVersion() =>
        Marshal.PtrToStringAnsi(FlircLibraryWrapper.fl_lib_version());

    public static string? FetchIrLibraryVersion() =>
        Marshal.PtrToStringAnsi(IrLibraryWrapper.ir_lib_version());

    public static bool OpenDevice(bool irMode)
    {
        try
        {
            if (irMode)
            {
                return FlircLibraryWrapper.fl_open_device_alt(VendorId, DeviceId) >= 0;
            }

            return FlircLibraryWrapper.fl_open_device(VendorId, DeviceId) >= 0;
        }
        catch
        {
            return false;
        }
    }

    /// <param name="timeout">In seconds (must be bigger than 0 to be accounted for)</param>
    public static bool WaitForDevice(int? timeout = null)
    {
        try
        {
            if (timeout is > 0)
            {
                return FlircLibraryWrapper.fl_wait_for_device_timeout(VendorId, DeviceId, timeout.Value) >= 0;
            }

            return FlircLibraryWrapper.fl_wait_for_device(VendorId, DeviceId) >= 0;
        }
        catch
        {
            return false;
        }
    }

    public static bool RegisterTransmitter()
    {
        var callback = new IrLibraryWrapper.IrRegisterTxCallback(FlircLibraryWrapper.fl_transmit_raw);
        return IrLibraryWrapper.ir_register_tx(callback) == 0;
    }

    /// <summary>
    /// Returns a response when it has successfully decoded a valid packet and returned a NOFRAME
    /// </summary>
    public static List<IrProt> ListenToPoll(CancellationToken token)
    {
        var protos = new List<IrProt>();
        var packet = new IrPacket();

        while (!token.IsCancellationRequested)
        {
            if (token.IsCancellationRequested)
                break;

            Console.WriteLine("Polling for packet...");
            var result = FlircLibraryWrapper.fl_ir_packet_poll(ref packet);
            Console.WriteLine($"Polled packet: {result}");
            switch (result)
            {
                case 0:
                    if (protos.Count > 0)
                    {
                        Debug.WriteLine("NOFRAME recieved after packets decoded, returning protos...");
                        return protos;
                    }
                    break;

                case 1:
                    var processed = new IrProt
                    {
                        buf = new ushort[256],
                        pronto = new ushort[256]
                    };
                    var proto = IrLibraryWrapper.ir_decode_packet(ref packet, ref processed);

                    Debug.WriteLine($"Received IR signal: Scancode = 0x{processed.scancode:X}, Protocol = {processed.protocol}, Repeat = {processed.repeat}");
                    Debug.WriteLine(GetBufValue(ref packet));

                    // if decoded packet is invalid, repeat or unknown, ignore it.
                    // UNSURE if there are other "proto"s that should be handled differently...
                    // Or if these are needed for some special protocol/sequence
                    if (proto is RcProto.RC_PROTO_INVALID or RcProto.RC_PROTO_NEC_REPEAT or RcProto.RC_PROTO_UNKNOWN)
                    {
                        Debug.WriteLine($"Dropping packet because of type: {proto}");
                        break;
                    }

                    protos.Add(processed);
                    break;

                case -1:
                    throw new Exception("Failed polling IR signal, disconnected...");

                default:
                    throw new Exception("Failed polling IR signal, unknown error...");
            }
        }

        // fallback, send back current list of packets
        return protos;
    }

    public static void CloseDevice()
    {
        FlircLibraryWrapper.fl_close_device();
    }

    public static nint SendPacket(short[] buf, bool retry = true)
    {
        try
        {
            Debug.WriteLine("[SEND PACKET] Transmit packet...");
            var result = TransmitRaw(buf);
            Debug.WriteLine($"[SEND PACKET] Transmitted packet... status: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Sending packet threw an exception: {ex.Message}...");

            if (!retry)
                return -1;
            Debug.WriteLine("Attempting Re-register transmitter and re-send...");
            if (RegisterTransmitter())
                return SendPacket(buf, false);

            Debug.WriteLine("Failed to add a new transmitter, packet not sent...");
            return -1;
        }
    }

    private static int TransmitRaw(short[] buf)
    {
        var unmanagedBuf = Marshal.AllocHGlobal(buf.Length * sizeof(short));

        try
        {
            Marshal.Copy(buf, 0, unmanagedBuf, buf.Length);
            return FlircLibraryWrapper.fl_transmit_raw(unmanagedBuf, Convert.ToUInt16(buf.Length), 0, 0);
        }
        finally
        {
            Marshal.FreeHGlobal(unmanagedBuf);
        }
    }

    private static string GetBufValue(ref IrPacket packet)
    {
        var value = new StringBuilder();
        for (var i = 0; i < packet.len && i < packet.buf.Length; i++)
        {
            value.Append($"{packet.buf[i]} ");
        }

        return value.ToString();
    }
}
