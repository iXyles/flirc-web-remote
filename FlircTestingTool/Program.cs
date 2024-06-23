using System.Diagnostics;
using FlircWrapper;

// dummy program for testing purposes
// used to build the wrappers around IR & Flirc libraries & communicatation with the actual device

var service = new FlircService();
IrProt? lastPacket = null;

Trace.Listeners.Add(new ConsoleTraceListener());

Console.WriteLine("Initializing Flirc...");
Console.WriteLine(service.FetchFlircLibraryVersion());
Console.WriteLine(service.FetchIrLibraryVersion());

// try open connection to device
var connected = service.OpenConnection();
if (connected)
{
    service.RegisterTransmitter();
    Console.WriteLine("Flirc device opened successfully...");
}

// if failed, maybe disconnected, then wait till it is connected
if (!connected)
{
    Console.WriteLine("Unable to connect, device not found (most likely), please plug it in...");
    var result = service.WaitForDevice();
    if (!result)
    {
        Console.WriteLine("Failed connecting, closing program...");
        return;
    }
    service.RegisterTransmitter();
    Console.WriteLine("Flirc device opened successfully...");
}

if (!service.RegisterTransmitter())
{
    Console.WriteLine("Failed to register transmit callback...");
    return;
}
Console.WriteLine("Transmitter callback successfully registered...");

Console.WriteLine("Listening to commands...");
Console.WriteLine("'send' to resend, 'listen' to record packet for send, 'exit' to close");
ListenForCommands();
Console.WriteLine("Closing connection to device...");
service.CloseConnection();

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
    var protos = service.ListenToPoll(CancellationToken.None);

    // return first "proto/packet" - this is from testing of my devices that has worked...
    // if we need to send more than a single packet for something, then this gotta be changed, along "mapped" object
    if (protos.Any())
    {
        lastPacket = protos.FirstOrDefault();
        Console.WriteLine("Packet recorded, can now be sent with 'send' command...");
    }
    else
    {
        Console.WriteLine("No packet recorded, type next command...");
    }
}

void TransmitRecordedPacket()
{
    if (!lastPacket.HasValue)
    {
        Console.WriteLine("No packet recorded to re-transmit.");
        return;
    }

    var result = service.SendPacket(lastPacket.Value);
    Console.WriteLine(result == 0 ? "Successfully sent packet..." : $"Failed to send packet, status code: {result}");
}
