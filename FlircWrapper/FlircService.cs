using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace FlircWrapper;

/// <summary>
/// Basic service to utilize underlaying libraries of Flirc
/// </summary>
public class FlircService
{
    const ushort VendorId = 0x20A0; // Flirc vendor ID
    const string DeviceId = "flirc.tv"; // Flirc device ID

    public string FetchFlircLibraryVersion() =>
        Marshal.PtrToStringAnsi(FlircSdkWrapper.fl_lib_version());

    public string FetchIrLibraryVersion() =>
        Marshal.PtrToStringAnsi(IrSdkWrapper.ir_lib_version());

    public bool OpenConnection()
    {
        try
        {
            return FlircSdkWrapper.fl_open_device_alt(VendorId, DeviceId) >= 0;
        }
        catch
        {
            return false;
        }
    }

    public bool WaitForDevice()
    {
        try
        {
            return FlircSdkWrapper.fl_wait_for_device(VendorId, DeviceId) >= 0;
        }
        catch
        {
            return false;
        }
    }

    public bool RegisterTransmitter()
    {
        IrSdkWrapper.IrRegisterTxCallback callback = FlircSdkWrapper.fl_transmit_raw;
        return IrSdkWrapper.ir_register_tx(callback) == 0;
    }

    /// <summary>
    /// Returns a response when it has successfully decoded a valid packet and returned a NOFRAME
    /// </summary>
    public List<IrProt> ListenToPoll(CancellationToken token)
    {
        var protos = new List<IrProt>();
        var packet = new IrPacket();

        while (!token.IsCancellationRequested)
        {
            if (token.IsCancellationRequested)
                break;

            Console.WriteLine("Polling for packet...");
            var result = FlircSdkWrapper.fl_ir_packet_poll(ref packet);
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
                    var proto = IrSdkWrapper.ir_decode_packet(ref packet, ref processed);

                    Debug.WriteLine($"Received IR signal: Scancode = 0x{processed.scancode:X}, Protocol = {processed.protocol}, Repeat = {processed.repeat}");
                    Debug.WriteLine(GetBufValue(ref packet));

                    // if decoded packet is invalid, repeat or unknown, ignore it.
                    // UNSURE if there are other "proto"s that should be handled differently...
                    // Or if these are needed for some special protocol/sequence
                    if (proto == RcProto.RC_PROTO_INVALID ||
                        proto == RcProto.RC_PROTO_NEC_REPEAT ||
                        proto == RcProto.RC_PROTO_UNKNOWN)
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

    public void CloseConnection()
    {
        FlircSdkWrapper.fl_close_device();
    }

    public nint SendPacket(IrProt packet)
    {
        // ensure we flush the interface of pending packets before sending a packet
        Debug.WriteLine("Flush interface of pending packets...");
        FlircSdkWrapper.fl_dev_flush();
        Debug.WriteLine("Transmit packet...");
        return IrSdkWrapper.ir_tx(packet.protocol, packet.scancode, packet.repeat);
    }

    private string GetBufValue(ref IrPacket packet)
    {
        var value = new StringBuilder();
        for (int i = 0; i < packet.len && i < packet.buf.Length; i++)
        {
            value.Append($"{packet.buf[i]} ");
        }

        return value.ToString();
    }
}
