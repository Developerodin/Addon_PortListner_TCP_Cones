using System.Net;
using System.Net.Sockets;
using System.Text;

// Configuration: Easily modify the weight value here
const double weightValue = 0.550;

IPAddress targetIP = IPAddress.Parse("127.0.0.1");
int targetPort = 4321; // Receiver listens on port 4321
int senderPort = 4321; // Sender binds to port 4321
IPEndPoint targetEndPoint = new IPEndPoint(targetIP, targetPort);
IPEndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, senderPort);

using UdpClient udpClient = new UdpClient(senderEndPoint);

Console.WriteLine($"Scale Simulator - Sending from port {senderPort} to {targetIP}:{targetPort}");
Console.WriteLine($"Weight: {weightValue} kg");
Console.WriteLine($"Interval: 500 ms");
Console.WriteLine("Press Ctrl+C to stop.\n");

while (true)
{
    // Format message exactly as: RTW:0.650 kg
    string message = $"RTW:{weightValue:F3} kg";
    byte[] data = Encoding.ASCII.GetBytes(message);
    
    udpClient.Send(data, data.Length, targetEndPoint);
    Console.WriteLine($"Sent: {message}");
    
    await Task.Delay(500); // Wait 500 milliseconds
}

