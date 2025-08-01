using System.Diagnostics;
using System.Runtime.InteropServices;
using FlircWrapper;

// dummy program for testing purposes
// used to build the wrappers around IR & Flirc libraries & communicatation with the actual device

IrProt? lastRecordedPacket = null;

Trace.Listeners.Add(new ConsoleTraceListener());

Console.WriteLine("Initializing Flirc...");
Console.WriteLine(FlircUtil.FetchFlircLibraryVersion());
Console.WriteLine(FlircUtil.FetchIrLibraryVersion());

// try open connection to device
var connected = FlircUtil.OpenDevice(true);
if (connected)
{
    FlircUtil.RegisterTransmitter();
    Console.WriteLine("Flirc device opened successfully...");
}

// if failed, maybe disconnected, then wait till it is connected
if (!connected)
{
    Console.WriteLine("Unable to connect, device not found (most likely), please plug it in...");
    var result = FlircUtil.WaitForDevice();
    if (!result)
    {
        Console.WriteLine("Failed connecting, closing program...");
        return;
    }

    FlircUtil.OpenDevice(true);
    FlircUtil.RegisterTransmitter();
    Console.WriteLine("Flirc device opened successfully...");
}

if (!FlircUtil.RegisterTransmitter())
{
    Console.WriteLine("Failed to register transmit callback...");
    return;
}
Console.WriteLine("Transmitter callback successfully registered...");

Console.WriteLine("Listening to commands...");
Console.WriteLine("'send' to resend, 'listen' to record packet for send, 'exit' to close");
ListenForCommands();
Console.WriteLine("Closing connection to device...");
FlircUtil.CloseDevice();

void ListenForCommands()
{
    while (true)
    {
        var command = Console.ReadLine() ?? string.Empty;
        if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
            break;

        if (command.Equals("listen", StringComparison.OrdinalIgnoreCase))
            RecordPacket();
        else if (command.Equals("send", StringComparison.OrdinalIgnoreCase))
            TransmitRecordedPacket();
        else
            Console.WriteLine("Unknown command. Available commands: 'send', 'listen', 'exit'.");
    }
    Console.WriteLine("Stopped listening for commands...");
}

void RecordPacket()
{
    Console.WriteLine("Listening, press a key on your remote...");
    var protos = FlircUtil.ListenToPoll(CancellationToken.None);

    // return first "proto/packet" - this is from testing of my devices that has worked...
    // if we need to send more than a single packet for something, then this gotta be changed, along "mapped" object
    if (protos.Any())
    {
        lastRecordedPacket = protos.FirstOrDefault();
        Console.WriteLine("Packet recorded, can now be sent with 'send' command...");
    }
    else
    {
        Console.WriteLine("No packet recorded, type next command...");
    }
}

void TransmitRecordedPacket()
{
    if (!lastRecordedPacket.HasValue)
    {
        Console.WriteLine("No packet recorded to re-transmit.");
        return;
    }

    // for test tool we flush before to ensure the IR endpoint is empty
    // in an actual app the regular "open" should be used for "normal usage"
    // and the "alt" should be used during "configuration mode"
    FlircLibraryWrapper.fl_dev_flush();

    var result = TransmitRaw(lastRecordedPacket.Value.buf, lastRecordedPacket.Value.len, 0, 0);
    Console.WriteLine(result == 0 ? "Successfully sent packet..." : $"Failed to send packet, status code: {result}");
}

static int TransmitRaw(ushort[] buf, ushort len, ushort ik, byte repeat)
{
    // Create a new buffer of the correct length
    var trimmed = new ushort[len];
    Array.Copy(buf, trimmed, len);

    // Convert to short[] for Marshal.Copy
    var shortBuf = Array.ConvertAll(trimmed, b => unchecked((short)b));
    var unmanagedBuf = Marshal.AllocHGlobal(shortBuf.Length * sizeof(short));

    try
    {
        Marshal.Copy(shortBuf, 0, unmanagedBuf, shortBuf.Length);
        return FlircLibraryWrapper.fl_transmit_raw(unmanagedBuf, len, ik, repeat);
    }
    finally
    {
        Marshal.FreeHGlobal(unmanagedBuf);
    }
}
